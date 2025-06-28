using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IEnableLoopAction"/>
public class EnableLoopAction(
    ILogger<EnableLoopAction> logger
) : IEnableLoopAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
                                   SocketGuildUser user,
                                   PlaybackContext ctx)
    {
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
