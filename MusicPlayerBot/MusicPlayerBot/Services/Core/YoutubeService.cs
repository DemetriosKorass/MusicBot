using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;
using YoutubeExplode;

namespace MusicPlayerBot.Services.Core;

/// <summary>
/// Fetches YouTube video metadata and resolves a playable audio URL,
/// with fallback to yt-dlp if necessary.
/// </summary>
public class YoutubeService(ILogger<YoutubeService> logger) : IYoutubeService
{
    private readonly YoutubeClient _yt = new();

    public async Task<Track> GetTrackAsync(string youtubeUrl)
    {
        var videoId = ExtractVideoId(youtubeUrl);
        if (videoId == null)
        {
            logger.LogError("Failed to extract video ID from URL: {Url}", youtubeUrl);
            throw new ArgumentException("Invalid YouTube URL provided.", nameof(youtubeUrl));
        }

        var metadata = await _yt.Videos.GetAsync(videoId);
        logger.LogInformation("Fetched metadata for {Url}: {Title}", youtubeUrl, metadata.Title);

        var streamUrl = await TryGetExplodeStreamAsync(videoId, youtubeUrl)
                         ?? await GetViaYtDlpAsync(youtubeUrl);

        return Track.FromInfo(videoId, streamUrl, metadata.Title) with
        {
            Duration = metadata.Duration,
            ThumbnailUrl = metadata.Thumbnails?.Count > 0 ? metadata.Thumbnails[0].Url : null
        };

    }

    private static string? ExtractVideoId(string url)
    {
        var m = Regex.Match(url, @"(?:v=|\/)([0-9A-Za-z_-]{11})(?:\?|&|$)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private async Task<string?> TryGetExplodeStreamAsync(string videoId, string youtubeUrl)
    {
        try
        {
            var hls = await _yt.Videos.Streams.GetHttpLiveStreamUrlAsync(videoId);
            if (!string.IsNullOrWhiteSpace(hls))
            {
                logger.LogInformation("Using HLS playlist for {Url}", youtubeUrl);
                return hls;
            }

            var manifest = await _yt.Videos.Streams.GetManifestAsync(videoId);
            var best = manifest.GetAudioOnlyStreams()
                               .OrderByDescending(s => s.Bitrate)
                               .FirstOrDefault();
            if (best != null)
            {
                logger.LogInformation(
                  "Selected audio-only stream for {Url}: type={type}, bitrate={Bitrate}",
                  youtubeUrl, best.Container.Name, best.Bitrate
                );
                return best.Url;
            }

            logger.LogWarning("No audio-only streams found for {Url}", youtubeUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "YoutubeExplode fetch failed for {Url}", youtubeUrl);
        }

        return null;
    }

    private Task<string> GetViaYtDlpAsync(string youtubeUrl)
        => Task.Run(() =>
        {
            logger.LogInformation("Falling back to yt-dlp for {Url}", youtubeUrl);

            var psi = new ProcessStartInfo("yt-dlp", $"--format bestaudio --get-url \"{youtubeUrl}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi) ?? throw new FileNotFoundException("yt-dlp not found");
            var url = proc.StandardOutput.ReadLine();
            proc.WaitForExit();

            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("yt-dlp did not return a URL.");

            return url;
        });
}
