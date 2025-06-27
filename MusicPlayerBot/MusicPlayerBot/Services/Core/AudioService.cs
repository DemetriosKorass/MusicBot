using Discord;
using Discord.Audio;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;

public class AudioService(
    IPlaybackContextManager ctxMgr,
    IAudioEncoder encoder,
    ILogger<AudioService> logger
) : IAudioService
{
    /// <summary>
    /// Raised when a track finishes playback.
    /// </summary>
    public event Func<ulong, string, Task>? TrackEnded;

    /// <inheritdoc />
    public async Task<IAudioClient> PlayAsync(IVoiceChannel channel, Track track)
    {
        var guildId = channel.Guild.Id;
        var ctx = ctxMgr.GetOrCreate(guildId);

        var audioClient = await ConnectAndPrepareAsync(channel, ctx);

        var pcm = audioClient.CreatePCMStream(AudioApplication.Mixed);

        CancelAndResetTrack(ctx);
        var token = ctx.TrackCts.Token;

        _ = Task.Run(() => RunPlaybackAsync(track, audioClient, pcm, guildId, token));

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
            try { await audioClient.StopAsync().ConfigureAwait(false); } catch { }

            logger.LogInformation("Guild {GuildId}: track ended: {Title}", guildId, track.Title);

            if (TrackEnded != null)
                await TrackEnded(guildId, track.Title);
        }
    }


    /// <inheritdoc />
    public Task SkipAsync(IGuild guild)
    {
        var ctx = ctxMgr.GetOrCreate(guild.Id);
        logger.LogInformation("Guild {GuildId}: skip invoked", guild.Id);
        ctx.TrackCts.Cancel();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(IGuild guild)
    {
        var ctx = ctxMgr.GetOrCreate(guild.Id);
        if (ctx.AudioClient is { })
        {
            logger.LogInformation("Guild {GuildId}: stop invoked", guild.Id);
            await ctx.AudioClient.StopAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task<string[]> GetQueueAsync(IGuild guild)
    {
        var ctx = ctxMgr.GetOrCreate(guild.Id);
        var titles = ctx.TrackQueue
                        .Select(t => t.Title)
                        .ToArray();
        return Task.FromResult(titles);
    }

    /// <inheritdoc />
    public Task<bool> ToggleLoopAsync(IGuild guild)
    {
        var ctx = ctxMgr.GetOrCreate(guild.Id);
        ctx.IsLoopEnabled = !ctx.IsLoopEnabled;
        logger.LogInformation(
            "Guild {GuildId}: loop {State}",
            guild.Id, ctx.IsLoopEnabled ? "enabled" : "disabled"
        );
        return Task.FromResult(ctx.IsLoopEnabled);
    }
}
