using Discord;
using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;

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
                .Build(),

            new SlashCommandBuilder()
                .WithName("loop")
                .WithDescription("Toggle loop mode for the current queue")
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
        Console.WriteLine($"[CommandHandler] Slash '{slash.Data.Name}' invoked by {slash.User.Username}");
        await slash.DeferAsync(ephemeral: false);

        try
        {
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

                        try
                        {
                            var title = await _youtube.GetVideoTitleAsync(videoUrl) ?? "Unknown title";
                            var streamUrl = await _youtube.GetAudioStreamUrlAsync(videoUrl)
                                              ?? throw new Exception("Couldn’t get an audio stream for that link.");

                            await _audio.PlayAsync(user.VoiceChannel, slash.Channel, streamUrl, title);
                            await slash.FollowupAsync($"▶️ Playing **{title}**");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[CommandHandler] Play error: {ex}");
                            await slash.FollowupAsync($"❌ Error playing track: {ex.Message}", ephemeral: true);
                        }

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
                            await slash.FollowupAsync("📃 Queue is empty.", ephemeral: true);
                        else
                        {
                            var list = string.Join("\n", queueTitles.Select((t, i) => $"{i + 1}. {t}"));
                            await slash.FollowupAsync($"📃 **Queue:**\n{list}", ephemeral: true);
                        }
                        break;
                    }

                case "loop":
                    {
                        var toggledOn = await _audio.ToggleLoopAsync(user.VoiceChannel.Guild);
                        await slash.FollowupAsync(
                            toggledOn
                              ? "🔁 Loop is **ON**."
                              : "↪️ Loop is **OFF**.",
                            ephemeral: true
                        );
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CommandHandler] Handler error: {ex}");
            await slash.FollowupAsync("❌ An unexpected error occurred.", ephemeral: true);
        }
    }
}
