using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;
/// <summary>
/// Coordinates all playback actions (Play, Stop, Skip, Queue, Loop) by
/// calling into YouTube and Audio services and sending a single Discord response.
/// </summary>
public interface IPlaybackOrchestrator
{
    /// <summary>
    /// Handles the Play command: resolves the video title and stream URL, 
    /// enqueues or starts playback, and sends a single response indicating
    /// whether the track is playing immediately or has been enqueued.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    /// <param name="videoUrl">The YouTube video URL to play.</param>
    Task AddTrackAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl);

    /// <summary>
    /// Signals the audio service to skip the current track and play the next one.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user);

    /// <summary>
    /// Stops playback entirely and clears the queue.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task StopAsync(SocketSlashCommand slash, SocketGuildUser user);

    /// <summary>
    /// Shortcut for skipping: invokes PlayNextAsync.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task SkipAsync(SocketSlashCommand slash, SocketGuildUser user);

    /// <summary>
    /// Shows the current queue of pending tracks.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task QueueAsync(SocketSlashCommand slash, SocketGuildUser user);

    /// <summary>
    /// Toggles loop mode for the queue, so finished tracks are re-enqueued.
    /// </summary>
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task LoopAsync(SocketSlashCommand slash, SocketGuildUser user);

    /// <summary>
    /// Checks whether command initiating user is in voice channel, if not - returns false.
    /// </summary>  
    /// <param name="slash">The incoming slash command context.</param>
    /// <param name="user">The guild user who invoked the command.</param>
    Task<bool> CheckIfUserIsInChannelAsync(SocketSlashCommand slash, SocketGuildUser user);
}
