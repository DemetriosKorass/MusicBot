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

        Client.Disconnected += async ex =>
        {
            Console.WriteLine($"⚠️ Gateway disconnected: {ex?.Message}. Reconnecting in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await Client.StartAsync();
            }
            catch (Exception reconnectEx)
            {
                Console.WriteLine($"❌ Reconnect failed: {reconnectEx.Message}");
            }
        };

        await Client.LoginAsync(TokenType.Bot, _cfg.Token);
        await Client.StartAsync();
    }
}