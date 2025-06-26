namespace MusicPlayerBot.Services.Interfaces;
public interface IYoutubeService
{
    Task<string?> GetAudioStreamUrlAsync(string youtubeUrl);
}