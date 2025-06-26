using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    public class AudioService : IAudioService
    {
        private class PlaybackContext
        {
            public IAudioClient AudioClient { get; set; } = null!;
            public Process FfmpegProcess { get; set; } = null!;
            public Task CopyTask { get; set; } = null!;
            public CancellationTokenSource Cts { get; set; } = null!;
            public Queue<string> TrackQueue { get; } = new();
            public bool IsPlaying { get; set; }
        }

        private readonly ConcurrentDictionary<ulong, PlaybackContext> _contexts
            = new();

        public async Task PlayAsync(IVoiceChannel vChannel, IMessageChannel textChannel, string url)
        {
            var guildId = vChannel.Guild.Id;

            if (!_contexts.TryGetValue(guildId, out var ctx))
            {
                ctx = new PlaybackContext();
                _contexts[guildId] = ctx;
            }

            if (ctx.IsPlaying)
            {
                ctx.TrackQueue.Enqueue(url);
                await textChannel.SendMessageAsync($"➡️ Enqueued. Position: {ctx.TrackQueue.Count}");
                return;
            }

            await InternalPlayAsync(vChannel, textChannel, url, ctx);
        }

        private async Task InternalPlayAsync(
            IVoiceChannel vChannel,
            IMessageChannel textChannel,
            string url,
            PlaybackContext ctx
        )
        {
            ctx.IsPlaying = true;

            var conn = await vChannel.ConnectAsync();
            var pcm = conn.CreatePCMStream(AudioApplication.Mixed);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-re -i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi)!;
            var cts = new CancellationTokenSource();

            ctx.AudioClient = conn;
            ctx.FfmpegProcess = proc;
            ctx.Cts = cts;

            ctx.CopyTask = Task.Run(async () =>
            {
                try
                {
                    using var output = proc.StandardOutput.BaseStream;
                    await output.CopyToAsync(pcm, cts.Token);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    await pcm.FlushAsync();
                    await conn.StopAsync();

                    ctx.IsPlaying = false;

                    if (ctx.TrackQueue.TryDequeue(out var nextUrl))
                    {
                        await InternalPlayAsync(vChannel, textChannel, nextUrl, ctx);
                    }
                    else
                    {
                        _contexts.TryRemove(vChannel.Guild.Id, out _);
                    }
                }
            });
        }

        public async Task StopAsync(IGuild guild)
        {
            var guildId = guild.Id;
            if (_contexts.TryRemove(guildId, out var ctx))
            {
                ctx.Cts.Cancel();
                try { await ctx.CopyTask; } catch { }
                if (!ctx.FfmpegProcess.HasExited) ctx.FfmpegProcess.Kill();
                await ctx.AudioClient.StopAsync();
            }
        }
    }
}
