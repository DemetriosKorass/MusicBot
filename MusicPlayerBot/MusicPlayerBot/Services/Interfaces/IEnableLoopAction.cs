using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces
{
    public interface IEnableLoopAction
    {
        Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user);
    }
}