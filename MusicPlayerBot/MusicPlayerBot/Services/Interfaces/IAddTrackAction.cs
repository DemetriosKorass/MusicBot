using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces
{
    /// <summary>  
    /// Interface for adding a track to the music player bot.  
    /// </summary>  
    public interface IAddTrackAction
    {
        /// <summary>  
        /// Executes the action to add a track to the music player.  
        /// </summary>  
        /// <param name="slash">The slash command triggering the action.</param>  
        /// <param name="user">The guild user requesting the action.</param>  
        /// <param name="youtubeUrl">The YouTube URL of the track to add.</param>  
        /// <returns>A task representing the asynchronous operation.</returns>  
        Task ExecuteAsync(SocketSlashCommand slash,
                          SocketGuildUser user,
                          string youtubeUrl,
                          PlaybackContext context);
    }
}