using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IShowQueueAction"/>
public class ShowQueueAction(ILogger<ShowQueueAction> logger) : IShowQueueAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
                                    SocketGuildUser user,
                                    PlaybackContext ctx)
    {
        var voiceChannel = user.VoiceChannel;
        var textChannel = slash.Channel;
        var items = ctx.TrackQueue
                       .Select((t, i) => $"{i + 1}. {t.DisplayName}")
                       .ToArray();

        if (items.Length == 0)
        {
            logger.LogInformation("Guild {Guild}: queue empty", user.Guild.Id);
            await slash.FollowupAsync("📃 Queue is empty.", ephemeral: true);
        }
        else
        {
            logger.LogInformation("Guild {Guild}: listing queue ({Count} items)", user.Guild.Id, items.Length);
            await slash.FollowupAsync(
                "📃 **Upcoming:**\n" + string.Join("\n", items),
                ephemeral: true
            );
        }
    }
}
