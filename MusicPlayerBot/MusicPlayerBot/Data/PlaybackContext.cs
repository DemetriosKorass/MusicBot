using Discord.Audio;

namespace MusicPlayerBot.Data;

/// <summary>
/// Holds all playback state for a single guild.
/// </summary>
public sealed class PlaybackContext : IAsyncDisposable
{
    public Track? CurrentTrack { get; set; }
    public Queue<Track> TrackQueue { get; } = new();
    public bool IsLoopEnabled { get; set; }
    public bool IsRunning { get; set; }
    public CancellationTokenSource TrackCts { get; private set; } = new();
    public IAudioClient? AudioClient { get; set; }

    public PlaybackContext() { }

    public async ValueTask DisposeAsync()
    {
        try { TrackCts.Cancel(); } catch { }
        if (AudioClient is not null)
            await AudioClient.StopAsync();
        TrackCts.Dispose();
    }
}
