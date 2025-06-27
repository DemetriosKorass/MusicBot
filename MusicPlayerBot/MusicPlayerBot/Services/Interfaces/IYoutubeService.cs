using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;
public interface IYoutubeService
{
    /// <summary>
    /// Retrieves metadata and the best-available audio stream URL for the given YouTube link.
    /// </summary>
    Task<Track> GetTrackAsync(string youtubeUrl);
}