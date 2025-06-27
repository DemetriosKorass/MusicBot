using Discord;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

public interface IAudioService
{
    /// <summary>
    /// Raised when a track starts playing.
    /// </summary>
    event Action<ulong, string> TrackStarted;

    /// <summary>
    /// Raised when a track finishes.
    /// </summary>
    event Action<ulong, string> TrackEnded;

    Task PlayAsync(IVoiceChannel vChannel, IMessageChannel textChannel, Track track);
    Task SkipAsync(IGuild guild);
    Task StopAsync(IGuild guild);
    Task<string[]> GetQueueAsync(IGuild guild);
    Task<bool> ToggleLoopAsync(IGuild guild);
    Task PauseAsync(IGuild guild);
    Task ResumeAsync(IGuild guild);
    Task<bool> IsPlayingAsync(IGuild guild);
}
