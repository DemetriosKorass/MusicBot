using YoutubeExplode;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;
public class YoutubeService : IYoutubeService
{
    private readonly YoutubeClient _yt = new();

    public async Task<string?> GetAudioStreamUrlAsync(string url)
    {
        var manifest = await _yt.Videos.Streams.GetManifestAsync(url);
        var audio = manifest.GetAudioOnlyStreams()
                            .OrderByDescending(s => s.Bitrate)
                            .FirstOrDefault();
        return audio?.Url;
    }
    public async Task<string?> GetVideoTitleAsync(string url)
    {
        try
        {
            var video = await _yt.Videos.GetAsync(url);
            return video.Title;
        }
        catch
        {
            return null;
        }
    }
}