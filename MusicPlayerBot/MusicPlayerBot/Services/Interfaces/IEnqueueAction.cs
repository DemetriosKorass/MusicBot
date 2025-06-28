using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>  
/// Represents an action to enqueue a track for playback.  
/// </summary>  
public interface IEnqueueAction
{
    /// <summary>  
    /// Executes the action to enqueue a track in the specified guild.  
    /// </summary>  
    /// <param name="slash">The slash command triggering the enqueue action.</param>  
    /// <param name="user">The user requesting the track enqueue.</param>  
    /// <param name="videoUrl">The URL of the video to enqueue.</param>  
    /// <param name="context">The playback context containing voice and text channels.</param>  
    /// <returns>A task representing the asynchronous operation.</returns>  
    Task ExecuteAsync(SocketSlashCommand slash,
                      SocketGuildUser user,
                      string videoUrl,
                      PlaybackContext context);
}
