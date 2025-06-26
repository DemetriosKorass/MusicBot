using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
public interface IDiscordService
{
    DiscordSocketClient Client { get; }
    Task StartAsync();
}