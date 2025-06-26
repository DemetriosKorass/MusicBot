using Discord;
using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;

public class CommandHandler(IAudioService audio, IYoutubeService youtube) : ICommandHandler
{
    private readonly IAudioService _audio = audio;
    private readonly IYoutubeService _youtube = youtube;
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
            new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("Play a YouTube URL")
                .AddOption("url",
                            ApplicationCommandOptionType.String,
                            "YouTube video URL",
                            isRequired: true)
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
                .Build()
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
        await slash.DeferAsync(ephemeral: false);

        var user = slash.User as SocketGuildUser;
        if (user?.VoiceChannel == null)
        {
            await slash.FollowupAsync(
                "You must be in a voice channel.",
                ephemeral: true
            );
            return;
        }

        switch (slash.Data.Name)
        {
            case "play":
                {
                    var videoUrl = slash.Data.Options
                                          .First(o => o.Name == "url")
                                          .Value!
                                          .ToString()!;

                    var title = await _youtube.GetVideoTitleAsync(videoUrl) ?? "Unknown title";
                    var streamUrl = await _youtube.GetAudioStreamUrlAsync(videoUrl);
                    if (streamUrl == null)
                    {
                        await slash.FollowupAsync(
                            "❌ Couldn’t get an audio stream for that link.",
                            ephemeral: true
                        );
                        return;
                    }

                    await _audio.PlayAsync(
                        user.VoiceChannel,
                        slash.Channel,
                        streamUrl,
                        title
                    );

                    await slash.FollowupAsync("▶️ Playing (or enqueued)!");
                    break;
                }

            case "stop":
                await _audio.StopAsync(user.VoiceChannel.Guild);
                await slash.FollowupAsync("⏹️ Stopped.");
                break;

            case "skip":
                await _audio.SkipAsync(user.VoiceChannel.Guild);
                await slash.FollowupAsync("⏭️ Skipped.");
                break;

            case "queue":
                {
                    var queueTitles = await _audio.GetQueueAsync(user.VoiceChannel.Guild);
                    if (queueTitles.Length == 0)
                    {
                        await slash.FollowupAsync("📃 Queue is empty.", ephemeral: true);
                    }
                    else
                    {
                        var list = string.Join("\n", queueTitles
                            .Select((t, i) => $"{i + 1}. {t}"));
                        await slash.FollowupAsync(
                            $"📃 **Queue:**\n{list}",
                            ephemeral: true
                        );
                    }
                    break;
                }
        }
    }
}
