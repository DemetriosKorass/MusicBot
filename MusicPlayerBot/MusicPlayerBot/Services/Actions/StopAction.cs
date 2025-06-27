using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IStopAction"/>
public class StopAction(
    IPlaybackContextManager ctxMgr,
    IAudioService audio,
    ILogger<StopAction> logger
) : IStopAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
        SocketGuildUser user,
        PlaybackContext ctx)
    {
        var guildId = user.Guild.Id;
        logger.LogInformation("Guild {Guild}: stop requested", guildId);

        await ctxMgr.RemoveAsync(guildId);
        await audio.StopAsync(user.Guild, ctx);
        await slash.FollowupAsync("⏹️ Playback stopped.");
    }
}
