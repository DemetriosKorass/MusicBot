
using System.Collections.Concurrent;
using Discord;
using Discord.Audio;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    public class AudioService : IAudioService
    {
        public event Action<ulong, string> TrackStarted = delegate { };
        public event Action<ulong, string> TrackEnded = delegate { };

        private readonly IAudioEncoder _encoder;
        private readonly ILogger<AudioService> _logger;
        private readonly ConcurrentDictionary<ulong, PlaybackContext> _contexts
            = new();

        public AudioService(IAudioEncoder encoder, ILogger<AudioService> logger)
        {
            _encoder = encoder;
            _logger = logger;
        }

        public Task PlayAsync(
            IVoiceChannel vChannel,
            IMessageChannel textChannel,
            string streamUrl,
            string title
        )
        {
            var guildId = vChannel.Guild.Id;
            var ctx = _contexts.GetOrAdd(guildId, _ => new PlaybackContext(vChannel));

            // enqueue into both channel and snapshot
            ctx.Queue.Writer.TryWrite((streamUrl, title));
            ctx.SnapshotQueue.Enqueue((streamUrl, title));

            // only start the loop once
            if (!ctx.IsRunning)
            {
                ctx.IsRunning = true;
                ctx.LoopCts = new CancellationTokenSource();
                _ = RunPlaybackLoop(guildId, ctx);
            }

            return Task.CompletedTask;
        }

        private async Task RunPlaybackLoop(ulong guildId, PlaybackContext ctx)
        {
            ctx.AudioClient = await ctx.VoiceChannel.ConnectAsync();
            var pcm = ctx.AudioClient.CreatePCMStream(AudioApplication.Mixed);

            // loop until LoopCts is cancelled
            await foreach (var (url, title) in ctx.Queue.Reader.ReadAllAsync(ctx.LoopCts.Token))
            {
                // respect pause
                while (ctx.IsPaused)
                    await Task.Delay(100, ctx.LoopCts.Token);

                // new TrackCts per track
                ctx.TrackCts?.Dispose();
                ctx.TrackCts = new CancellationTokenSource();

                TrackStarted?.Invoke(guildId, title);
                _logger.LogInformation("Track started: {Title}", title);

                try
                {
                    // cancel only this track
                    await _encoder.EncodeToPcmAsync(url, pcm, ctx.TrackCts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Track {Title} was skipped.", title);
                }

                TrackEnded?.Invoke(guildId, title);
                _logger.LogInformation("Track ended: {Title}", title);

                await pcm.FlushAsync();

                // re-enqueue if looping
                if (ctx.IsLoopEnabled)
                    ctx.Queue.Writer.TryWrite((url, title));
            }

            // loop has ended
            ctx.IsRunning = false;
            try { await pcm.FlushAsync(); } catch { }
            await ctx.DisposeAsync();
            _contexts.TryRemove(guildId, out _);
        }

        public Task SkipAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
            {
                // cancel only the current track
                ctx.TrackCts.Cancel();
            }
            return Task.CompletedTask;
        }

        public async Task StopAsync(IGuild guild)
        {
            if (_contexts.TryRemove(guild.Id, out var ctx))
            {
                // cancel the entire loop
                ctx.LoopCts.Cancel();
                await ctx.DisposeAsync();
            }
        }

        public Task<string[]> GetQueueAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
            {
                var titles = ctx.SnapshotQueue.Select(t => t.Title).ToArray();
                return Task.FromResult(titles);
            }
            return Task.FromResult(Array.Empty<string>());
        }

        public Task<bool> ToggleLoopAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
            {
                ctx.IsLoopEnabled = !ctx.IsLoopEnabled;
                return Task.FromResult(ctx.IsLoopEnabled);
            }
            else
            {
                var newCtx = new PlaybackContext(null!);
                newCtx.IsLoopEnabled = true;
                _contexts[guild.Id] = newCtx;
                return Task.FromResult(true);
            }
        }

        public Task PauseAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
                ctx.IsPaused = true;
            return Task.CompletedTask;
        }

        public Task ResumeAsync(IGuild guild)
        {
            if (_contexts.TryGetValue(guild.Id, out var ctx))
                ctx.IsPaused = false;
            return Task.CompletedTask;
        }
    }
}
