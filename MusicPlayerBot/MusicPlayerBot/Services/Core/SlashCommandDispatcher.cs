using Discord;
using Discord.WebSocket;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core
{
    public class SlashCommandDispatcher(
        ICommandHandler<PlayCommand> playHandler,
        ICommandHandler<SkipCommand> skipHandler,
        ICommandHandler<StopCommand> stopHandler,
        ICommandHandler<QueueCommand> queueHandler,
        ICommandHandler<LoopCommand> loopHandler,
        IPlaybackContextManager ctxMgr
    ) : ISlashCommandDispatcher
    {
        private readonly ICommandHandler<PlayCommand> _playHandler = playHandler;
        private readonly ICommandHandler<SkipCommand> _skipHandler = skipHandler;
        private readonly ICommandHandler<StopCommand> _stopHandler = stopHandler;
        private readonly ICommandHandler<QueueCommand> _queueHandler = queueHandler;
        private readonly ICommandHandler<LoopCommand> _loopHandler = loopHandler;
        private DiscordSocketClient _client = null!;

        public Task Initialize(DiscordSocketClient client)
        {
            _client = client;
            _client.Ready += RegisterSlashCommands;
            _client.SlashCommandExecuted += OnSlashExecuted;
            return Task.CompletedTask;
        }

        private async Task RegisterSlashCommands()
        {
            var commands = new[]
            {
                new SlashCommandBuilder().WithName("play").WithDescription("Play a YouTube URL (or enqueue if already playing)")
                    .AddOption("url", ApplicationCommandOptionType.String, "YouTube video URL", isRequired: true).Build(),
                new SlashCommandBuilder().WithName("stop").WithDescription("Stop playback").Build(),
                new SlashCommandBuilder().WithName("skip").WithDescription("Skip the current track").Build(),
                new SlashCommandBuilder().WithName("queue").WithDescription("Show pending tracks").Build(),
                new SlashCommandBuilder().WithName("loop").WithDescription("Toggle loop mode").Build()
            };

            foreach (var cmd in commands)
                await _client.CreateGlobalApplicationCommandAsync(cmd);
        }

        private Task OnSlashExecuted(SocketSlashCommand slash)
        {
            _ = HandleSlashAsync(slash);
            return Task.CompletedTask;
        }

        private async Task HandleSlashAsync(SocketSlashCommand slash)
        {
            var user = slash.User as SocketGuildUser;
            var ctx = ctxMgr.GetOrCreate(user!.Guild.Id, user.VoiceChannel, slash.Channel);

            if (user?.VoiceChannel == null)
            {
                await slash.RespondAsync("You must be in a voice channel.", ephemeral: true);
                return;
            }

            switch (slash.Data.Name)
            {
                case "play":
                    var playCmd = new PlayCommand(
                        slash,
                        user,
                        slash.Data.Options.First(o => o.Name == "url").Value!.ToString()!
                    );
                    await _playHandler.HandleAsync(playCmd, ctx);
                    break;

                case "skip":
                    var skipCmd = new SkipCommand(slash, user);
                    await _skipHandler.HandleAsync(skipCmd, ctx);
                    break;

                case "stop":
                    var stopCmd = new StopCommand(slash, user);
                    await _stopHandler.HandleAsync(stopCmd, ctx);
                    break;

                case "queue":
                    var queueCmd = new QueueCommand(slash, user);
                    await _queueHandler.HandleAsync(queueCmd, ctx);
                    break;

                case "loop":
                    var loopCmd = new LoopCommand(slash, user);
                    await _loopHandler.HandleAsync(loopCmd, ctx);
                    break;
            }
        }
    }
}