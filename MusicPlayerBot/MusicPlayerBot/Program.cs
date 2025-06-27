using Microsoft.Extensions.DependencyInjection;
using MusicPlayerBot.Services.Interfaces;
using MusicPlayerBot.Services.Core;
using MusicPlayerBot.Services;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.CommandHandlers;

class Program
{
    static async Task Main()
    {
        var services = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.IncludeScopes = false;
                });
            })
            .AddSingleton<IConfigurationService, ConfigurationService>()
            .AddSingleton<IDiscordService, DiscordService>()
            .AddSingleton<IYoutubeService, YoutubeService>()
            .AddSingleton<IAudioService, AudioService>()
            .AddSingleton<IAudioEncoder, FFmpegAudioEncoder>()
            .AddSingleton<IPlaybackOrchestrator, PlaybackOrchestrator>()
            .AddSingleton<ICommandHandler_, SlashCommandDispatcher>()
            .AddSingleton<ICommandHandler<PlayCommand>, PlayCommandHandler>()
            .AddSingleton<ICommandHandler<SkipCommand>, SkipCommandHandler>()
            .AddSingleton<ICommandHandler<StopCommand>, StopCommandHandler>()
            .AddSingleton<ICommandHandler<QueueCommand>, QueueCommandHandler>()
            .AddSingleton<ICommandHandler<LoopCommand>, LoopCommandHandler>();

        var provider = services.BuildServiceProvider();
        var discord = provider.GetRequiredService<IDiscordService>();
        var cmds = provider.GetRequiredService<ICommandHandler_>();

        await discord.StartAsync();
        await cmds.Initialize(discord.Client);

        await Task.Delay(-1);
    }
}
