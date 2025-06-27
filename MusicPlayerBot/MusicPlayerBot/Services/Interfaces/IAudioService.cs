using Discord;
using Discord.Audio;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Defines audio playback operations for voice channels,
/// including playing tracks, skipping, stopping, queue inspection, and loop control.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Raised whenever a track finishes playback (either naturally or via skip).
    /// </summary>
    /// <param name="guildId">The ID of the guild where the track ended.</param>
    /// <param name="trackTitle">The title of the track that ended.</param>
    event Func<ulong, string, Task> TrackEnded;

    /// <summary>
    /// Plays the specified <see cref="Track"/> in the given voice channel.
    /// </summary>
    /// <param name="channel">The voice channel to play audio in.</param>
    /// <param name="track">The track to play, containing metadata and stream URL.</param>
    /// <returns>The <see cref="IAudioClient"/> used for playback.</returns>
    Task<IAudioClient> PlayAsync(IVoiceChannel channel, Track track);

    /// <summary>
    /// Cancels the currently playing track in the specified guild, causing playback to end.
    /// </summary>
    /// <param name="guild">The guild whose current track should be skipped.</param>
    Task SkipAsync(IGuild guild);

    /// <summary>
    /// Stops all playback in the specified guild and disconnects from the voice channel.
    /// </summary>
    /// <param name="guild">The guild whose playback should be stopped.</param>
    Task StopAsync(IGuild guild);

    /// <summary>
    /// Retrieves the titles of all tracks currently enqueued for playback in the specified guild.
    /// </summary>
    /// <param name="guild">The guild whose queue should be inspected.</param>
    /// <returns>An array of track titles in queue order.</returns>
    Task<string[]> GetQueueAsync(IGuild guild);

    /// <summary>
    /// Toggles loop mode for the playback queue in the specified guild.
    /// When enabled, tracks re-enter the queue after ending.
    /// </summary>
    /// <param name="guild">The guild whose loop mode to toggle.</param>
    /// <returns>True if loop mode is now enabled; otherwise, false.</returns>
    Task<bool> ToggleLoopAsync(IGuild guild);
}
