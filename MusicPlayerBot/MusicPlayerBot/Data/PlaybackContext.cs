using Discord.Audio;

namespace MusicPlayerBot.Data;

/// <summary>
/// Holds all playback state for a single guild.
/// </summary>
public sealed class PlaybackContext : IAsyncDisposable
{
    private CancellationTokenSource _trackCts = new();
    public Track? CurrentTrack { get; set; }
    public Queue<Track> TrackQueue { get; } = new();
    public bool IsLoopEnabled { get; set; }
    public bool IsRunning { get; set; }
    public CancellationTokenSource TrackCts => _trackCts; 
    public void ResetTrackCts() => _trackCts = new CancellationTokenSource(); 

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
