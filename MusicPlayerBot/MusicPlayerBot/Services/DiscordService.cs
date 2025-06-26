using Discord;
using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;
public class DiscordService : IDiscordService
{
    public DiscordSocketClient Client { get; }
    private readonly IConfigurationService _cfg;

    public DiscordService(IConfigurationService cfg)
    {
        _cfg = cfg;
        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildVoiceStates
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent
        });
    }

    public async Task StartAsync()
    {
        Client.Log += msg =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        await Client.LoginAsync(TokenType.Bot, _cfg.Token);
        await Client.StartAsync();
    }
}