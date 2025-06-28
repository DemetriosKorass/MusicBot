using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces
{
    /// <summary>
    /// Represents an action to display the music queue in the Discord bot.
    /// </summary>
    public interface IShowQueueAction
    {
        /// <summary>
        /// Executes the action to show the music queue.
        /// </summary>
        /// <param name="slash">The slash command triggering the action.</param>
        /// <param name="user">The guild user who initiated the command.</param>
        /// <param name="context">The playback context containing the current track and queue information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(SocketSlashCommand slash,
                          SocketGuildUser user,
                          PlaybackContext context);
    }
}