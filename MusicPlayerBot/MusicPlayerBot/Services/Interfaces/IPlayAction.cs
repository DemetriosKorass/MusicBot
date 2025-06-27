using Discord;
using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Represents an action to play the next track in the queue (AKA skip).
/// </summary>
public interface IPlayAction
{
    /// <summary>
    /// Executes the action to play the next track in the specified guild and voice channel.
    /// </summary>
    /// <param name="guild">The guild where the next track will be played.</param>
    /// <param name="voiceChannel">The voice channel where the next track will be played.</param>
    /// <param name="textChannel">The text channel for sending playback notifications.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(SocketSlashCommand slash,
                             SocketGuildUser user,
                             string videoUrl);
}
