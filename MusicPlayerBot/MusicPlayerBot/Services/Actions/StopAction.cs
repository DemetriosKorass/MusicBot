using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

public class StopAction(
    IPlaybackContextManager ctxMgr,
    IAudioService audio,
    ILogger<StopAction> logger
) : IStopAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user)
    {
        await slash.DeferAsync(ephemeral: false);

        var guildId = user.Guild.Id;
        logger.LogInformation("Guild {Guild}: stop requested", guildId);

        await ctxMgr.RemoveAsync(guildId);
        await audio.StopAsync(user.Guild);
        await slash.FollowupAsync("⏹️ Playback stopped.");
    }
}
