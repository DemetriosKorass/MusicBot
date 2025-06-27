using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IAddTrackAction"/>  
public class AddTrackAction(
   IYoutubeService yt,
   IPlayAction playNow,
   ILogger<AddTrackAction> logger
) : IAddTrackAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
                                   SocketGuildUser user,
                                   string youtubeUrl,
                                   PlaybackContext ctx)
    {
        Track track = await yt.GetTrackAsync(youtubeUrl);

        ctx.TrackQueue.Enqueue(track);
        logger.LogInformation(
            "Guild {GuildId}: enqueued track {Title} (queue size now {Count})",
            user.Guild.Id, track.Title, ctx.TrackQueue.Count
        );

        await playNow.ExecuteAsync(track, ctx, slash);
    }
}
