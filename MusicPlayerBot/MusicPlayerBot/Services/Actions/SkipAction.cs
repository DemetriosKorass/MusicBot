using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="ISkipAction"/>
public class SkipAction(
    IPlaybackContextManager ctxMgr,
    ILogger<SkipAction> logger
) : ISkipAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user)
    {
        var ctx = ctxMgr.GetOrCreate(user.Guild.Id);
        logger.LogInformation("Guild {Guild}: skip requested", user.Guild.Id);

        ctx.TrackCts.Cancel();
        await slash.FollowupAsync("⏭️ Skipped.");
    }
}
