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
    /// <remarks>  
    /// This event is triggered after the playback of a track concludes,  
    /// allowing subscribers to perform cleanup or queue management tasks.  
    /// </remarks>  
    event Func<ulong,PlaybackContext, string, Task> TrackEnded;

    /// <summary>  
    /// Plays the specified <see cref="Track"/> in the given voice channel.  
    /// </summary>  
    /// <param name="track">The track to play, containing metadata and stream URL.</param>  
    /// <param name="context">The playback context, including voice and text channels.</param>  
    /// <returns>The <see cref="IAudioClient"/> used for playback.</returns>  
    /// <exception cref="InvalidOperationException">Thrown if playback cannot be started.</exception>  
    /// <remarks>  
    /// Initiates playback of the provided track in the specified voice channel.  
    /// Ensure the <paramref name="context"/> is properly initialized before calling this method.  
    /// </remarks>  
    Task<IAudioClient> PlayAsync(Track track, PlaybackContext context);

    /// <summary>  
    /// Cancels the currently playing track in the specified guild, causing playback to end.  
    /// </summary>  
    /// <param name="guild">The guild whose current track should be skipped.</param>  
    /// <param name="ctx">The playback context, including voice and text channels.</param>  
    /// <remarks>  
    /// This method skips the currently playing track and moves to the next track in the queue,  
    /// if available. If no tracks remain, playback stops.  
    /// </remarks>  
    Task SkipAsync(IGuild guild, PlaybackContext ctx);

    /// <summary>  
    /// Stops all playback in the specified guild and disconnects from the voice channel.  
    /// </summary>  
    /// <param name="guild">The guild whose playback should be stopped.</param>  
    /// <param name="ctx">The playback context, including voice and text channels.</param>  
    /// <remarks>  
    /// This method halts playback entirely and clears the track queue.  
    /// The bot will also disconnect from the voice channel.  
    /// </remarks>  
    Task StopAsync(IGuild guild, PlaybackContext ctx);
}
