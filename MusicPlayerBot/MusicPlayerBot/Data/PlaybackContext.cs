using System.Collections.Concurrent;
using System.Threading.Channels;
using Discord;
using Discord.Audio;

namespace MusicPlayerBot.Data;

internal class PlaybackContext : IAsyncDisposable
{
    public IVoiceChannel VoiceChannel { get; }
    public Channel<(string Url, string Title)> Queue { get; }
        = Channel.CreateUnbounded<(string, string)>();
    public ConcurrentQueue<(string Url, string Title)> SnapshotQueue { get; }
        = new ConcurrentQueue<(string, string)>();
    public IAudioClient AudioClient { get; set; } = null!;

    /// <summary>
    /// Cancels the entire playback loop (used by Stop).
    /// </summary>
    public CancellationTokenSource LoopCts { get; set; } = new();

    /// <summary>
    /// Cancels only the current track decode (used by Skip).
    /// </summary>
    public CancellationTokenSource TrackCts { get; set; } = new();

    public bool IsLoopEnabled { get; set; }
    public bool IsPaused { get; set; }
    public bool IsRunning { get; set; }

    public PlaybackContext(IVoiceChannel channel) => VoiceChannel = channel;

    public async ValueTask DisposeAsync()
    {
        try { LoopCts.Cancel(); } catch { }
        try { await AudioClient.StopAsync(); } catch { }
        AudioClient.Dispose();
        TrackCts.Dispose();
        LoopCts.Dispose();
    }
}
