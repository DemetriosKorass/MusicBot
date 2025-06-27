using Discord;
using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Represents an action to stop playback.
/// </summary>
public interface IStopAction
{
    /// <summary>
    /// Executes the action to stop playback in the specified guild.
    /// </summary>
    /// <param name="slash">The slash command triggering the stop action.</param>  
    /// <param name="user">The guild user requesting the stop action.</param> 
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(SocketSlashCommand slash,
                      SocketGuildUser user);
}
