using System.Collections.Concurrent;
using System.Diagnostics;
using Discord;
using Discord.Audio;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    public class AudioService : IAudioService
    {
        private record Track(string StreamUrl, string Title);
        private class PlaybackContext
        {
            public IAudioClient AudioClient { get; set; } = null!;
            public Process FfmpegProcess { get; set; } = null!;
            public Task CopyTask { get; set; } = null!;
            public CancellationTokenSource Cts { get; set; } = null!;
            public Queue<Track> TrackQueue { get; } = new();
            public bool IsPlaying { get; set; }
        }

        private readonly ConcurrentDictionary<ulong, PlaybackContext> _contexts
            = new();

        public async Task PlayAsync(IVoiceChannel vChannel, IMessageChannel textChannel, string streamUrl, string title)
        {
            var guildId = vChannel.Guild.Id;

            if (!_contexts.TryGetValue(guildId, out var ctx))
            {
                ctx = new PlaybackContext();
                _contexts[guildId] = ctx;
            }

            var track = new Track(streamUrl, title);

            if (ctx.IsPlaying)
            {
                ctx.TrackQueue.Enqueue(track);
                await textChannel.SendMessageAsync($"➡️ Enqueued **{title}**. Position: {ctx.TrackQueue.Count}");
                return;
            }

            await InternalPlayAsync(vChannel, ctx, track);
        }

        private async Task InternalPlayAsync(
            IVoiceChannel vChannel,
            PlaybackContext ctx,
            Track track
        )
        {
            ctx.IsPlaying = true;

            var conn = await vChannel.ConnectAsync();
            var pcm = conn.CreatePCMStream(AudioApplication.Mixed);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-re -i \"{track.StreamUrl}\" -ac 2 -f s16le -ar 48000 pipe:1",
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

                    if (ctx.TrackQueue.TryDequeue(out var next))
                    {
                        await InternalPlayAsync(vChannel, ctx, next);
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
            if (_contexts.TryRemove(guild.Id, out var ctx))
            {
                ctx.Cts.Cancel();
                try { await ctx.CopyTask; } catch { }
                if (!ctx.FfmpegProcess.HasExited) ctx.FfmpegProcess.Kill();
                await ctx.AudioClient.StopAsync();
            }
        }

        public Task SkipAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx) && ctx.IsPlaying)
            {
                ctx.Cts.Cancel();
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetQueueAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
            {
                return Task.FromResult(ctx.TrackQueue.Select(t => t.Title).ToArray());
            }

            return Task.FromResult(Array.Empty<string>());
        }
    }
}
