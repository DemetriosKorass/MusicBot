using Discord;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;
using System.Collections.Concurrent;

namespace MusicPlayerBot.Services;

public class PlaybackContextManager : IPlaybackContextManager
{
    private readonly ConcurrentDictionary<ulong, PlaybackContext> _store
      = new();

    public PlaybackContext GetOrCreate(ulong guildId, IVoiceChannel voiceChannel, IMessageChannel textChannel)
        => _store.GetOrAdd(guildId, id => new PlaybackContext(voiceChannel, textChannel));

  
    public async ValueTask RemoveAsync(ulong guildId)
    {
        if (_store.TryRemove(guildId, out var ctx))
        {
            await ctx.DisposeAsync();
        }
    }

}
