using Discord;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

public interface IPlaybackContextManager
{
    /// <summary>  
    /// Retrieves or creates the shared playback context for the specified guild.  
    /// </summary>  
    /// <param name="guildId">The unique identifier of the guild.</param>  
    /// <param name="voiceChannel">The voice channel associated with the playback context.</param>  
    /// <param name="textChannel">The text channel associated with the playback context.</param>  
    /// <returns>A <see cref="PlaybackContext"/> instance representing the playback context.</returns>  
    PlaybackContext GetOrCreate(ulong guildId, IVoiceChannel voiceChannel, IMessageChannel textChannel);

    /// <summary>  
    /// Removes and disposes of the playback context for the specified guild.  
    /// </summary>  
    /// <param name="guildId">The unique identifier of the guild.</param>  
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>  
    ValueTask RemoveAsync(ulong guildId);
}
