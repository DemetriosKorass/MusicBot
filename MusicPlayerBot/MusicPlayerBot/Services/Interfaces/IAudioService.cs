using Discord;

namespace MusicPlayerBot.Services.Interfaces;
public interface IAudioService
{
    Task PlayAsync(IVoiceChannel channel, IMessageChannel textChannel, string streamUrl, string title);
    Task StopAsync(IGuild guild);
    Task SkipAsync(IGuild guild);
    Task<string[]> GetQueueAsync(IGuild guild);
}