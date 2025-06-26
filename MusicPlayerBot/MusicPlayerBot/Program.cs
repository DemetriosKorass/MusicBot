using Microsoft.Extensions.DependencyInjection;
using MusicPlayerBot.Services.Interfaces;
using MusicPlayerBot.Services;

class Program
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddSingleton<IConfigurationService, ConfigurationService>()
            .AddSingleton<IDiscordService, DiscordService>()
            .AddSingleton<IYoutubeService, YoutubeService>()
            .AddSingleton<IAudioService, AudioService>()
            .AddSingleton<ICommandHandler, CommandHandler>();

        var provider = services.BuildServiceProvider();
        var discord = provider.GetRequiredService<IDiscordService>();
        var cmds = provider.GetRequiredService<ICommandHandler>();

        await discord.StartAsync();
        await cmds.Initialize(discord.Client);

        await Task.Delay(-1);
    }
}
