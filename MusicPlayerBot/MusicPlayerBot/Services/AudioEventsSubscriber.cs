using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;

public class AudioEventsSubscriber
{
    private readonly IAudioService _audio;
    private readonly IPlayAction _playNext;
    private readonly IEnqueueAction _enqueue;
    private readonly DiscordSocketClient _client;

    public AudioEventsSubscriber(
        IAudioService audio,
        IPlayAction playNext,
        IEnqueueAction enqueue,
        DiscordSocketClient client
    )
    {
        _audio = audio;
        _playNext = playNext;
        _enqueue = enqueue;
        _client = client;

        _audio.TrackEnded += async (guildId, title) =>
            await OnTrackEndedAsync(guildId, title);
    }

    private async Task OnTrackEndedAsync(ulong guildId, string title)
    {
        var guild = _client.GetGuild(guildId);
        if (guild == null) return;

        // 2) Find the bot's voice channel in that guild
        var voiceState = guild
            .GetUser(_client.CurrentUser.Id)?
            .VoiceChannel;
        if (voiceState == null) return;

        // 3) Pick a text channel to send follow-up messages
        //    Here we use the first TextChannel, but you can store
        //    “last used” per guild for more precision.
        var textChannel = guild.TextChannels.FirstOrDefault();
        if (textChannel == null) return;

        // 4) Check loop flag
        var isLooping = await _audio.IsLoopEnabledAsync(guild);
        if (isLooping)
        {
            // re-enqueue the same title
            await _enqueue.ExecuteAsync(
                guild,
                textChannel,
                title
            );
        }
        else
        {
            // otherwise, play the next track
            await _playNext.ExecuteAsync(
                guild,
                voiceState,
                textChannel
            );
        }
    }
}
