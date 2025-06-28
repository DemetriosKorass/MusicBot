using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;

public class AudioEventsSubscriber
{
    private readonly IAudioService _audio;
    private readonly IPlayAction _play;
    private readonly ILogger<AudioEventsSubscriber> _logger;

    public AudioEventsSubscriber(
        IAudioService audio,
        IPlayAction play,
        ILogger<AudioEventsSubscriber> logger
    )
    {
        _audio = audio;
        _play = play;
        _logger = logger;

        _audio.TrackEnded += async (guildId, ctx, _)
            => await OnTrackEndedAsync(guildId, ctx);
    }

    private async Task OnTrackEndedAsync(ulong guildId, PlaybackContext ctx)
    {
        var last = ctx.CurrentTrack;
        _logger.LogInformation("Guild {GuildId}: track ended: {Title}", guildId, last?.Title);

        if (ctx.IsLoopEnabled && last != null)
        {
            ctx.TrackQueue.Enqueue(last);
            _logger.LogInformation("Guild {GuildId}: re-enqueued {Title} (loop)", guildId, last.Title);
        }

        if (ctx.TrackQueue.TryDequeue(out var next))
        {
            await _play.ExecuteAsync(next, ctx);
        }
        else
        {
            _logger.LogInformation("Guild {GuildId}: queue empty, stopping", guildId);
            ctx.IsRunning = false;
            ctx.CurrentTrack = null;
        }
    }
}
