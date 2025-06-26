using Discord;

namespace MusicPlayerBot.Services.Interfaces;
public interface IAudioService
{
    Task PlayAsync(IVoiceChannel channel, IMessageChannel textChannel, string streamUrl);
    Task StopAsync(IGuild guild);
}