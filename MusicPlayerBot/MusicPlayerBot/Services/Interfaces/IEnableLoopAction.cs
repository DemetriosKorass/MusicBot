using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces
{
    /// <summary>  
    /// Interface for enabling loop functionality in the music player bot.  
    /// </summary>  
    public interface IEnableLoopAction
    {
        /// <summary>  
        /// Executes the loop enabling action.  
        /// </summary>  
        /// <param name="slash">The slash command triggering the action.</param>  
        /// <param name="user">The guild user who initiated the command.</param>  
        /// <param name="ctx">The playback context containing the current state of playback.</param>  
        /// <returns>A task representing the asynchronous operation.</returns>  
        Task ExecuteAsync(SocketSlashCommand slash,
                          SocketGuildUser user,
                          PlaybackContext ctx);
    }
}