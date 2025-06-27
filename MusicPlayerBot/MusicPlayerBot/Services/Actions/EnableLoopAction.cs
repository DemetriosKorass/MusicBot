using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IEnableLoopAction"/>
public class EnableLoopAction(
    IPlaybackContextManager ctxMgr,
    ILogger<EnableLoopAction> logger
) : IEnableLoopAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user)
    {
        var ctx = ctxMgr.GetOrCreate(user.Guild.Id);
        ctx.IsLoopEnabled = !ctx.IsLoopEnabled;

        logger.LogInformation("Guild {Guild}: loop {State}",
            user.Guild.Id, ctx.IsLoopEnabled ? "enabled" : "disabled");

        await slash.FollowupAsync(
            ctx.IsLoopEnabled
              ? "🔁 Loop enabled."
              : "↪️ Loop disabled.",
            ephemeral: true
        );
    }
}
