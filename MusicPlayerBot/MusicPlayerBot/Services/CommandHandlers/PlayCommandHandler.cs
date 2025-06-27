using MusicPlayerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.CommandHandlers;

public class PlayCommandHandler(
    IYoutubeService yt,
    IAudioService audio,
    ILogger<PlayCommandHandler> log
    )
    : ICommandHandler<PlayCommand>
{
    public async Task HandleAsync(PlayCommand cmd)
    {
        await cmd.Slash.DeferAsync();

        log.LogInformation("PlayCommand for {User}: {Url}", cmd.User.Username, cmd.Url);

        var title = await yt.GetVideoTitleAsync(cmd.Url) ?? "Unknown title";
        var streamUrl = await yt.GetAudioStreamUrlAsync(cmd.Url)
                          ?? throw new Exception("Couldn’t get an audio stream.");

        var wasPlaying = await audio.IsPlayingAsync(cmd.User.VoiceChannel.Guild);
        await audio.PlayAsync(cmd.User.VoiceChannel, cmd.Slash.Channel, streamUrl, title);

        if (!wasPlaying)
            await cmd.Slash.FollowupAsync($"▶️ Now playing — **{title}**");
        else
        {
            var queue = await audio.GetQueueAsync(cmd.User.VoiceChannel.Guild);
            await cmd.Slash.FollowupAsync(
                $"➡️ Enqueued **{title}**. Position: {queue.Length}"
            );
        }
    }
}
