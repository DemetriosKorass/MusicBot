using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

public class PlayAction(
    IYoutubeService yt,
    IPlaybackContextManager ctxMgr,
    IAudioService audio,
    ILogger<PlayAction> logger
) : IPlayAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl)
    {
        await slash.DeferAsync(ephemeral: false);

        var track = await yt.GetTrackAsync(videoUrl);
        var ctx = ctxMgr.GetOrCreate(user.Guild.Id);

        logger.LogInformation("Guild {Guild}: starting track {Title}", user.Guild.Id, track.Title);

        ctx.IsRunning = true;
        ctx.CurrentTrack = track;
        ctx.AudioClient = await audio.PlayAsync(user.VoiceChannel, slash.Channel, track);

        await slash.FollowupAsync($"▶️ Now playing {track.DisplayName}");
    }
}
