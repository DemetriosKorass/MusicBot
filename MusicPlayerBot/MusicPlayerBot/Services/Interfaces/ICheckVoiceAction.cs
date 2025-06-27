using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Defines a contract for checking if the invoking user is in a voice channel.
/// Replies to the user if they are not in a voice channel.
/// </summary>
public interface ICheckVoiceAction
{
    /// <summary>
    /// Executes the voice channel check for the specified user and command.
    /// </summary>
    /// <param name="slash">The slash command invoked by the user.</param>
    /// <param name="user">The guild user to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the user is in a voice channel.</returns>
    Task<bool> ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user);
}
