using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="ISkipAction"/>
public class SkipAction(
    IAudioService audio,
    ILogger<SkipAction> logger
) : ISkipAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
        SocketGuildUser user,
        PlaybackContext ctx)
    {
        logger.LogInformation("Guild {Guild}: skip requested", user.Guild.Id);

        await audio.SkipAsync(user.Guild, ctx);
        await slash.FollowupAsync("⏭️ Skipped.");
    }
}
