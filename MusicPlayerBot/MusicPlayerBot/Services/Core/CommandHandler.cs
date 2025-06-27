using Discord;
using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;

/// <summary>
/// Handles incoming Discord slash commands and delegates to the playback orchestrator.
/// </summary>
public class CommandHandler(IPlaybackOrchestrator orchestrator) : ICommandHandler
{
    private readonly IPlaybackOrchestrator _orchestrator = orchestrator;
    private DiscordSocketClient _client = null!;

    /// <summary>
    /// Wires up the Discord client events.
    /// </summary>
    public Task Initialize(DiscordSocketClient client)
    {
        _client = client;
        _client.Ready += RegisterSlashCommands;
        _client.SlashCommandExecuted += OnSlashExecuted;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers all of the slash commands that the bot supports.
    /// </summary>
    private async Task RegisterSlashCommands()
    {
        var commands = new[]
        {
            new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("Play a YouTube URL (or enqueue if already playing)")
                .AddOption("url", ApplicationCommandOptionType.String, "YouTube video URL", isRequired: true)
                .Build(),

            new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Stop playback")
                .Build(),

            new SlashCommandBuilder()
                .WithName("skip")
                .WithDescription("Skip the current track and play the next in queue")
                .Build(),

            new SlashCommandBuilder()
                .WithName("queue")
                .WithDescription("Show all pending tracks in the queue")
                .Build(),

            new SlashCommandBuilder()
                .WithName("loop")
                .WithDescription("Toggle loop mode for the current queue")
                .Build()
        };

        foreach (var cmd in commands)
            await _client.CreateGlobalApplicationCommandAsync(cmd);
    }

    /// <summary>
    /// Entry point for when a slash command is executed.
    /// </summary>
    private Task OnSlashExecuted(SocketSlashCommand slash)
    {
        _ = HandleSlashAsync(slash);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Routes each slash command to the corresponding orchestrator action.
    /// </summary>
    private async Task HandleSlashAsync(SocketSlashCommand slash)
    {
        if (slash.User is not SocketGuildUser user || !_orchestrator.CheckIfUserIsInChannelAsync(slash, user).Result)
            return;

        switch (slash.Data.Name)
        {
            case "play":
                {
                    var videoUrl = slash.Data.Options
                                          .First(o => o.Name == "url")
                                          .Value!
                                          .ToString()!;
                    await _orchestrator.AddTrackAsync(slash, user, videoUrl);
                    break;
                }

            case "stop":
                await _orchestrator.StopAsync(slash, user);
                break;
            case "skip":
                await _orchestrator.SkipAsync(slash, user);
                break;
            case "queue":
                await _orchestrator.QueueAsync(slash, user);
                break;
            case "loop":
                await _orchestrator.LoopAsync(slash, user);
                break;
        }
    }
}
