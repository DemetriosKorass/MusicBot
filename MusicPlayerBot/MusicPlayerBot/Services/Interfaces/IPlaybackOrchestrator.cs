using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
public interface IPlaybackOrchestrator
{
    Task AddTrackAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl);
    Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user);
}
