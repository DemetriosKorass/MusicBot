using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>  
/// Starts playback of a fully-resolved <see cref="Track"/> in the specified channels.  
/// Invoked both on first-play and on end-of-track events.  
/// </summary>  
public interface IPlayAction
{
    /// <summary>  
    /// Immediately plays the given <paramref name="track"/>. Updates the shared PlaybackContext.  
    /// </summary>  
    /// <param name="track">The track to be played.</param>  
    /// <param name="ctx">The playback context to manage shared state.</param>  
    /// <param name="slash">Optional. The slash command that triggered the playback, if applicable.</param>  
    Task ExecuteAsync(Track track,
                      PlaybackContext ctx,
                      SocketSlashCommand? slash = null);
}
