using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

public class PlayAction(
   IAudioService audio,
   ILogger<PlayAction> logger
) : IPlayAction
{
    public async Task ExecuteAsync(Track track,
                                  PlaybackContext ctx,
                                  SocketSlashCommand? slash = null)
    {
        var guildId = ctx.VoiceChannel.Guild.Id;

        ctx.IsRunning = true;
        ctx.CurrentTrack = track;

        logger.LogInformation(
            "Guild {GuildId}: starting playback of {Title}",
            guildId, track.Title
        );

        ctx.AudioClient = await audio.PlayAsync(track, ctx);
  
        if (slash == null)
        {
            await ctx.TextChannel.SendMessageAsync($"▶️ Now playing {track.DisplayName}");
        }
        else
        {
            await slash.FollowupAsync($"▶️ Now playing {track.DisplayName}");
        }
    }
}
