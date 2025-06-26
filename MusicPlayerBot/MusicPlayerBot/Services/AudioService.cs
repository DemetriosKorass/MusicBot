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
            public IVoiceChannel Channel { get; set; } = null!;
            public IAudioClient AudioClient { get; set; } = null!;
            public CancellationTokenSource CurrentCts { get; set; } = null!;
            public Queue<Track> Queue { get; } = new();
            public bool IsRunning { get; set; }
        }

        private readonly ConcurrentDictionary<ulong, PlaybackContext> _contexts
            = new();

        public async Task PlayAsync(IVoiceChannel vChannel, IMessageChannel textChannel, string streamUrl, string title)
        {
            var guildId = vChannel.Guild.Id;
            var track = new Track(streamUrl, title);
            var ctx = _contexts.GetOrAdd(guildId, _ => new PlaybackContext { Channel = vChannel });

            ctx.Queue.Enqueue(track);
            await textChannel.SendMessageAsync(
                ctx.IsRunning
                  ? $"➡️ Enqueued **{title}**. Position: {ctx.Queue.Count}"
                  : $"▶️ Starting **{title}**..."
            );

            if (!ctx.IsRunning)
                _ = RunPlaybackLoop(ctx, guildId);
        }

        private async Task RunPlaybackLoop(PlaybackContext ctx, ulong guildId)
        {
            ctx.IsRunning = true;

            ctx.AudioClient = await ctx.Channel.ConnectAsync();
            var pcm = ctx.AudioClient.CreatePCMStream(AudioApplication.Mixed);

            while (ctx.Queue.TryDequeue(out var track))
            {
                ctx.CurrentCts = new CancellationTokenSource();

                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-re -i \"{track.StreamUrl}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                })!;

                try
                {
                    await proc.StandardOutput.BaseStream
                              .CopyToAsync(pcm, ctx.CurrentCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    try { if (!proc.HasExited) proc.Kill(); } catch { }
                }
            }

            try { await pcm.FlushAsync(); } catch { }
            try { await ctx.AudioClient.StopAsync(); } catch { }

            _contexts.TryRemove(guildId, out _);
        }

        public Task SkipAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx) && ctx.IsRunning)
            {
                ctx.CurrentCts.Cancel();
            }
            return Task.CompletedTask;
        }

        public async Task StopAsync(IGuild guild)
        {
            if (_contexts.TryRemove(guild.Id, out var ctx))
            {
                ctx.CurrentCts.Cancel();
                ctx.Queue.Clear();
                try { await ctx.AudioClient.StopAsync(); } catch { }
            }
        }

        public Task<string[]> GetQueueAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
                return Task.FromResult(ctx.Queue.Select(t => t.Title).ToArray());

            return Task.FromResult(Array.Empty<string>());
        }
    }
}
