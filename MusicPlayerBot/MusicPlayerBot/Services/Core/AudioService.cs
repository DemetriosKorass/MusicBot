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
        var audioClient = await ConnectAndPrepareAsync(ctx.VoiceChannel, ctx);

        var pcm = audioClient.CreatePCMStream(AudioApplication.Mixed);

        CancelAndResetTrack(ctx);
        var token = ctx.TrackCts.Token;

        _ = Task.Run(() => RunPlaybackAsync(track, audioClient, pcm, guildId, ctx, token));

        logger.LogInformation("Guild {GuildId}: started track: {Title}", guildId, track.Title);
        return audioClient;
    }

    private static async Task<IAudioClient> ConnectAndPrepareAsync(IVoiceChannel channel, PlaybackContext ctx)
    {
        var audioClient = await channel.ConnectAsync();
        ctx.AudioClient = audioClient;
        return audioClient;
    }

    private static void CancelAndResetTrack(PlaybackContext ctx)
    {
        ctx.TrackCts.Cancel();
        ctx.ResetTrackCts();
    }

    private async Task RunPlaybackAsync(
        Track track,
        IAudioClient audioClient,
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
            //try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }

            logger.LogInformation("Guild {GuildId}: track ended: {Title}", guildId, track.Title);

            if (TrackEnded != null)
                await TrackEnded(guildId, ctx, track.Title);
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
    public async Task StopAsync(IGuild guild, PlaybackContext ctx)
    {
        if (ctx.AudioClient is { })
        {
            logger.LogInformation("Guild {GuildId}: stop invoked", guild.Id);
            await ctx.AudioClient.StopAsync().ConfigureAwait(false);
        }
    }
}
