using Discord.WebSocket;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Handles incoming Discord slash commands and delegates to the playback orchestrator.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Wires up the Discord client events.
    /// </summary>
    Task Initialize(DiscordSocketClient client);
}