using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
public interface ICommandHandler
{
    Task Initialize(DiscordSocketClient client);
}