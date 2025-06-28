using Discord.Audio;
using Discord;

namespace MusicPlayerBot.Data;

public sealed class PlaybackContext(IVoiceChannel channel, IMessageChannel textChannel) : IAsyncDisposable
{
    public IVoiceChannel VoiceChannel { get; } = channel;
    public IMessageChannel TextChannel { get; set; } = textChannel;
    public Track? CurrentTrack { get; set; }
    public Queue<Track> TrackQueue { get; } = new();
    public bool IsLoopEnabled { get; set; }
    public bool IsRunning { get; set; }
    public CancellationTokenSource TrackCts { get; private set; } = new();
    public IAudioClient? AudioClient { get; set; }
    public AudioOutStream PcmStream { get; internal set; }

    public void ResetTrackCts()
    {
        TrackCts?.Dispose();
        TrackCts = new CancellationTokenSource();
    }

    public async ValueTask DisposeAsync()
    {
        try { TrackCts.Cancel(); } catch { }
        if (AudioClient is not null)
            await AudioClient.StopAsync();
        TrackCts.Dispose();
    }
}