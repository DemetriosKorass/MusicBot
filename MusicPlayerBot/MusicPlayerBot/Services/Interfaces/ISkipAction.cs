using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces
{
    /// <summary>
    /// Represents an action to skip the current track in the music player bot.
    /// </summary>
    public interface ISkipAction
    {
        /// <summary>
        /// Executes the skip action asynchronously.
        /// </summary>
        /// <param name="slash">The slash command triggering the skip action.</param>
        /// <param name="user">The guild user requesting the skip action.</param>
        /// <param name="context">The playback context containing the current track and queue.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(SocketSlashCommand slash,
                          SocketGuildUser user,
                          PlaybackContext context);
    }
}