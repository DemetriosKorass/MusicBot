using Discord;
using Discord.Audio;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;

public class AudioService(
    IAudioEncoder encoder,
    ILogger<AudioService> logger
) : IAudioService
{
    /// <summary>
    /// Raised when a track finishes playback.
    /// </summary>
    public event Func<ulong, PlaybackContext, string, Task>? TrackEnded;

    /// <inheritdoc />
    public async Task<IAudioClient> PlayAsync(Track track, PlaybackContext ctx)
    {
        var guildId = ctx.VoiceChannel.Guild.Id;

        if (ctx.AudioClient == null)
        {
            ctx.AudioClient = await ctx.VoiceChannel.ConnectAsync().ConfigureAwait(false);
            ctx.PcmStream = ctx.AudioClient.CreatePCMStream(AudioApplication.Mixed);
        }

        ctx.ResetTrackCts();
        var token = ctx.TrackCts.Token;

        _ = Task.Run(() => RunPlaybackAsync(track, ctx.PcmStream, guildId, ctx, token));

        logger.LogInformation("Guild {GuildId}: started track: {Title}", guildId, track.Title);
        return ctx.AudioClient;
    }

    private async Task RunPlaybackAsync(
        Track track,
        AudioOutStream pcm,
        ulong guildId,
        PlaybackContext ctx,
        CancellationToken token)
    {
        try
        {
            await encoder.EncodeToPcmAsync(track.StreamUrl, pcm, token);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Guild {GuildId}: track {Title} was skipped", guildId, track.Title);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Guild {GuildId}: error during playback of {Title}", guildId, track.Title);
        }
        finally
        {
            try { await pcm.FlushAsync(token).ConfigureAwait(false); } catch { }

            logger.LogInformation("Guild {GuildId}: track ended: {Title}", guildId, track.Title);

            if (TrackEnded != null)
                await TrackEnded(guildId, ctx, track.Title).ConfigureAwait(false);
        }
    }


    /// <inheritdoc />
    public Task SkipAsync(IGuild guild, PlaybackContext ctx)
    {
        logger.LogInformation("Guild {GuildId}: skip invoked", guild.Id);
        ctx.TrackCts.Cancel();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(IGuild guild, PlaybackContext ctx)
    {
        if (ctx.AudioClient is { })
        {
            ctx.TrackCts.Cancel();

            ctx.TrackQueue.Clear();

            ctx.IsRunning = false;
            logger.LogInformation("Guild {GuildId}: stop invoked", guild.Id);
        }
        return Task.CompletedTask;
    }
}
