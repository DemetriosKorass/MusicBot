namespace MusicPlayerBot.Services.Interfaces;
public interface IConfigurationService
{
    string Token { get; }
    string FfmpegPath { get; }
}