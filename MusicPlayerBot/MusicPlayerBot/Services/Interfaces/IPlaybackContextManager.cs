using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

public interface IPlaybackContextManager
{
    /// <summary>
    /// Get (or create) the shared context for this guild.
    /// </summary>
    PlaybackContext GetOrCreate(ulong guildId);

    /// <summary>
    /// Remove and dispose the context for this guild.
    /// </summary>
    ValueTask RemoveAsync(ulong guildId);
}
