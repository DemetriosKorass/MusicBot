using Microsoft.Extensions.DependencyInjection;
using MusicPlayerBot.Services.Interfaces;
using MusicPlayerBot.Services.Core;
using MusicPlayerBot.Services;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.CommandHandlers;
using MusicPlayerBot.Services.Actions;

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
            .AddSingleton<AudioEventsSubscriber>()
            .AddSingleton<IConfigurationService, ConfigurationService>()
            .AddSingleton<IDiscordService, DiscordService>()
            .AddSingleton<IYoutubeService, YoutubeService>()
            .AddSingleton<IAudioService, AudioService>()
            .AddSingleton<IAudioEncoder, FFmpegAudioEncoder>()
            .AddSingleton<IPlaybackOrchestrator, PlaybackOrchestrator>()
            .AddSingleton<IPlaybackContextManager, PlaybackContextManager>()

            .AddSingleton<ISlashCommandDispatcher, SlashCommandDispatcher>()
            .AddSingleton<ICommandHandler<PlayCommand>, PlayCommandHandler>()
            .AddSingleton<ICommandHandler<SkipCommand>, SkipCommandHandler>()
            .AddSingleton<ICommandHandler<StopCommand>, StopCommandHandler>()
            .AddSingleton<ICommandHandler<QueueCommand>, QueueCommandHandler>()
            .AddSingleton<ICommandHandler<LoopCommand>, LoopCommandHandler>()

            .AddSingleton<IPlayAction, PlayAction>()
            .AddSingleton<ISkipAction, SkipAction>()
            .AddSingleton<IStopAction, StopAction>()
            .AddSingleton<IEnqueueAction, EnqueueAction>()
            ;

        var provider = services.BuildServiceProvider();
        var discord = provider.GetRequiredService<IDiscordService>();
        var cmds = provider.GetRequiredService<ISlashCommandDispatcher>();

        await discord.StartAsync();
        await cmds.Initialize(discord.Client);

        await Task.Delay(-1);
    }
}
