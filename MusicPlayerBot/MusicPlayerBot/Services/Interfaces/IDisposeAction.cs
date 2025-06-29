using Discord.WebSocket;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces
{
    public interface IDisposeAction
    {
        Task ExecuteAsync(SocketSlashCommand slash,
                  SocketGuildUser user,
                  PlaybackContext ctx);
    }
}
