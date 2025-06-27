using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using MusicPlayerBot.Services.Interfaces;
using System.Diagnostics;

namespace MusicPlayerBot.Services.Core;
public class YoutubeService : IYoutubeService
{
    private readonly YoutubeClient _yt;
    private readonly HttpClient _http;
    private readonly string[] _invidiousInstances =
    [
        "https://yewtu.be",
        "https://yewtu.eu",
        "https://yewtu.onl",
        "https://yewtu.work"
    ];

    public YoutubeService()
    {
        var cookies = new CookieContainer();
        cookies.Add(new Uri("https://www.youtube.com"),
                    new Cookie("CONSENT", "YES+"));

        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        _http = new HttpClient(handler);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/115.0 Safari/537.36"
        );
        _http.DefaultRequestHeaders.Referrer = new Uri("https://www.youtube.com");

        _yt = new YoutubeClient(_http);
    }

    public async Task<string?> GetAudioStreamUrlAsync(string url)
    {
        var videoId = ExtractVideoId(url)
                   ?? throw new Exception("Invalid YouTube URL");

        try
        {
            var hls = await _yt.Videos.Streams.GetHttpLiveStreamUrlAsync(videoId);
            if (!string.IsNullOrEmpty(hls))
            {
                //Console.WriteLine($"[YoutubeService] Using HLS playlist: {hls}");
                return hls;
            }
        }
        catch { }

        try
        {
            var manifest = await _yt.Videos.Streams.GetManifestAsync(videoId);
            var best = SelectBest(manifest);
            if (best != null)
            {
                Console.WriteLine($"[YoutubeService] Selected direct stream: {best}");
                return best;
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            Console.WriteLine("[YoutubeService] YouTube 403 – falling back to Invidious");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[YoutubeService] Direct fetch failed: {ex.Message}");
        }

        foreach (var baseUrl in _invidiousInstances)
        {
            try
            {
                var apiUrl = $"{baseUrl}/api/v1/videos/{videoId}";
                var json = await _http.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("adaptiveFormats", out var arr))
                {
                    var streams = arr.EnumerateArray()
                        .Select(e => new
                        {
                            Url = e.GetProperty("url").GetString()!,
                            Container = e.GetProperty("container").GetString()!,
                            Bitrate = e.GetProperty("bitrate").GetInt32()
                        })
                        .ToList();
                    var best = streams
                        .Where(s => s.Container.Equals("mp4", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(s => s.Bitrate)
                        .FirstOrDefault()
                        ?? streams.OrderByDescending(s => s.Bitrate).FirstOrDefault();
                    if (best != null)
                    {
                        Console.WriteLine($"[YoutubeService] Invidious ({baseUrl}) → {best.Url}");
                        return best.Url;
                    }
                }
                Console.WriteLine($"[YoutubeService] Invidious ({baseUrl}) no formats");
            }
            catch (HttpRequestException hre) when (hre.StatusCode == (HttpStatusCode)429)
            {
                Console.WriteLine($"[YoutubeService] Invidious ({baseUrl}) 429 – trying next");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[YoutubeService] Invidious ({baseUrl}) error: {ex.Message}");
            }
        }

        try
        {
            Console.WriteLine("[YoutubeService] Falling back to yt-dlp");
            return await Task.Run(() => GetViaYtDlp(url));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("❌ yt-dlp not found. Please install yt-dlp and add it to your PATH.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[YoutubeService] yt-dlp failed: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetVideoTitleAsync(string url)
    {
        try
        {
            var id = ExtractVideoId(url);
            if (id == null) return null;
            var video = await _yt.Videos.GetAsync(id);
            return video.Title;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[YoutubeService] Title lookup failed: {ex.Message}");
            return null;
        }
    }

    private static string? ExtractVideoId(string url)
    {
        var m = Regex.Match(url, @"(?:v=|\/)([0-9A-Za-z_-]{11})(?:\?|&|$)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string? SelectBest(StreamManifest manifest)
    {
        var muxed = manifest.GetMuxedStreams()
            .Where(s => s.Container == Container.Mp4)
            .OrderByDescending(s => s.Bitrate)
            .FirstOrDefault();
        if (muxed != null) return muxed.Url;

        var audioMp4 = manifest.GetAudioOnlyStreams()
            .Where(s => s.Container == Container.Mp4)
            .OrderByDescending(s => s.Bitrate)
            .FirstOrDefault();
        if (audioMp4 != null) return audioMp4.Url;

        var webm = manifest.GetAudioOnlyStreams()
            .OrderByDescending(s => s.Bitrate)
            .FirstOrDefault();
        return webm?.Url;
    }

    private static string GetViaYtDlp(string pageUrl)
    {
        var psi = new ProcessStartInfo("yt-dlp",
            $"--format bestaudio --get-url \"{pageUrl}\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = Process.Start(psi)!;
        var line = proc.StandardOutput.ReadLine();
        proc.WaitForExit();
        if (string.IsNullOrWhiteSpace(line))
            throw new FileNotFoundException("yt-dlp did not return a URL.");
        return line;
    }
}