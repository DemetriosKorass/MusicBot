using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
/// <summary>
/// Defines the high-level playback operations that coordinate YouTube, audio streaming, and Discord messaging.
/// </summary>
public interface IPlaybackOrchestrator
{
    Task AddTrackAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl);
    Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user);
    Task StopAsync(SocketSlashCommand slash, SocketGuildUser user);
    Task SkipAsync(SocketSlashCommand slash, SocketGuildUser user);
    Task QueueAsync(SocketSlashCommand slash, SocketGuildUser user);
    Task LoopAsync(SocketSlashCommand slash, SocketGuildUser user);
    Task<bool> CheckIfUserIsInChannelAsync(SocketSlashCommand slash, SocketGuildUser user);
}
