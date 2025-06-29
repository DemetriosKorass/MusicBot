﻿using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="IEnqueueAction"/>
public class EnqueueAction(
    IYoutubeService yt,
    ILogger<EnqueueAction> logger
) : IEnqueueAction
{
    public async Task ExecuteAsync(SocketSlashCommand slash,
                                   SocketGuildUser user,
                                   string videoUrl,
                                   PlaybackContext ctx)
    {
        var track = await yt.GetTrackAsync(videoUrl);
        ctx.TrackQueue.Enqueue(track);

        logger.LogInformation("Guild {Guild}: enqueued {Title} (pos {Pos})",
            user.Guild.Id, track.Title, ctx.TrackQueue.Count);

        await slash.FollowupAsync(
            $"➡️ Enqueued {track.DisplayName}. Position: {ctx.TrackQueue.Count}"
        );
    }
}
