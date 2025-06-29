using Microsoft.Extensions.DependencyInjection;
using MusicPlayerBot.Services.Interfaces;
using MusicPlayerBot.Services.Core;
using MusicPlayerBot.Services;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.CommandHandlers;
using MusicPlayerBot.Services.Actions;
using Discord.WebSocket;

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
            .AddSingleton<IPlaybackContextManager, PlaybackContextManager>()

            .AddSingleton<ISlashCommandDispatcher, SlashCommandDispatcher>()
            .AddSingleton<ICommandHandler<PlayCommand>, PlayCommandHandler>()
            .AddSingleton<ICommandHandler<SkipCommand>, SkipCommandHandler>()
            .AddSingleton<ICommandHandler<StopCommand>, StopCommandHandler>()
            .AddSingleton<ICommandHandler<QueueCommand>, QueueCommandHandler>()
            .AddSingleton<ICommandHandler<LoopCommand>, LoopCommandHandler>()
            .AddSingleton<ICommandHandler<DisposeCommand>, DisposeCommandHandler>()

            .AddSingleton<IPlayAction, PlayAction>()
            .AddSingleton<ISkipAction, SkipAction>()
            .AddSingleton<IStopAction, StopAction>()
            .AddSingleton<IEnqueueAction, EnqueueAction>()
            .AddSingleton<IAddTrackAction, AddTrackAction>()
            .AddSingleton<ICheckVoiceAction, CheckVoiceAction>()
            .AddSingleton<IShowQueueAction, ShowQueueAction>()
            .AddSingleton<IEnableLoopAction, EnableLoopAction>()
            .AddSingleton<IDisposeAction, IDisposeAction>()

            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<AudioEventsSubscriber>();

        var provider = services.BuildServiceProvider();
        var discord = provider.GetRequiredService<IDiscordService>();
        var cmds = provider.GetRequiredService<ISlashCommandDispatcher>();
        provider.GetRequiredService<AudioEventsSubscriber>();

        await discord.StartAsync();
        await cmds.Initialize(discord.Client);

        await Task.Delay(-1);
    }
}
