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

        RegisterSlashCommandsOnReady();
        HookSlashCommandExecution();

        return Task.CompletedTask;
    }

    private void RegisterSlashCommandsOnReady()
    {
        _client.Ready += async () =>
        {
            var playCmd = new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("Play a YouTube URL")
                .AddOption("url",
                           ApplicationCommandOptionType.String,
                           "YouTube video URL",
                           isRequired: true)
                .Build();

            var stopCmd = new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Stop playback")
                .Build();

            await _client.CreateGlobalApplicationCommandAsync(playCmd);
            await _client.CreateGlobalApplicationCommandAsync(stopCmd);
        };
    }

    private void HookSlashCommandExecution()
    {
        _client.SlashCommandExecuted += slash =>
        {
            _ = slash.RespondAsync(
                slash.Data.Name == "play"
                    ? "▶️ Playing now!"
                    : "⏹️ Stopped."
            );

            _ = Task.Run(async () =>
            {
                switch (slash.Data.Name)
                {
                    case "play":
                        var url = slash.Data.Options
                                         .First(o => o.Name == "url")
                                         .Value!
                                         .ToString()!;
                        if (slash.User is SocketGuildUser user && user.VoiceChannel != null)
                        {
                            var stream = await _youtube.GetAudioStreamUrlAsync(url);
                            if (stream != null)
                                await _audio.PlayAsync(user.VoiceChannel, slash.Channel, stream);
                        }
                        break;

                    case "stop":
                        if (slash.User is SocketGuildUser usr && usr.VoiceChannel != null)
                            await _audio.StopAsync(usr.VoiceChannel.Guild);
                        break;
                }
            });

            return Task.CompletedTask;
        };
    }
}
