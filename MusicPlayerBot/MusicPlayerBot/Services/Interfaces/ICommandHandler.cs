using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
public interface ICommandHandler
{
    void Initialize(DiscordSocketClient client);
}