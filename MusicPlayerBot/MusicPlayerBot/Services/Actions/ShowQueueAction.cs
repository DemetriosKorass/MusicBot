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

        if (ctx.CurrentTrack == null && items.Length == 0)
        {
            logger.LogInformation("Guild {Guild}: no tracks currently playing or in queue", user.Guild.Id);
            await slash.FollowupAsync("📃 No tracks are currently playing or in the queue.", ephemeral: true);
        }
        else
        {
            var currentlyPlaying = ctx.CurrentTrack != null
                ? $" **Currently Playing:** {ctx.CurrentTrack.DisplayName}"
                : " **Currently Playing:** None";

            var playingNext = items.Length > 0
                ? "\n\n📃 **Playing Next:**\n" + string.Join("\n", items)
                : "";

            logger.LogInformation("Guild {Guild}: showing currently playing and queue ({Count} items)", user.Guild.Id, items.Length);
            await slash.FollowupAsync($"{currentlyPlaying}{playingNext}", ephemeral: true);
        }
    }
}
