using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;
public class CommandHandler(IAudioService audio, IYoutubeService youtube) : ICommandHandler
{
    private const char PREFIX = '!';

    public void Initialize(DiscordSocketClient client)
    {
        client.MessageReceived += async msgRaw =>
        {
            if (msgRaw.Author.IsBot || msgRaw is not SocketUserMessage msg) return;
            int pos = 0;
            if (!msg.HasCharPrefix(PREFIX, ref pos)) return;

            var parts = msg.Content[pos..].Split(' ', 2);
            var cmd = parts[0].ToLower();
            var arg = parts.Length > 1 ? parts[1] : null;

            switch (cmd)
            {
                case "play":
                    await HandlePlay(msg, arg);
                    break;
                case "stop":
                    await HandleStop(msg);
                    break;
            }
        };
    }

    private async Task HandlePlay(SocketUserMessage msg, string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) 
        {
            await msg.Channel.SendMessageAsync("Usage: `!play <YouTube-URL>`");
            return;
        }

        if (msg.Author is not IGuildUser user || user.VoiceChannel == null)
        {
            await msg.Channel.SendMessageAsync("Join a voice channel first.");
            return;
        }

        var streamUrl = await youtube.GetAudioStreamUrlAsync(url);
        if (streamUrl == null)
        {
            await msg.Channel.SendMessageAsync("Couldn’t get stream URL.");
            return;
        }

        await audio.PlayAsync(user.VoiceChannel, msg.Channel, streamUrl);
    }

    private async Task HandleStop(SocketUserMessage msg)
    {
        if (msg.Author is not IGuildUser user || user.VoiceChannel == null)
        {
            await msg.Channel.SendMessageAsync("You’re not in a voice channel.");
            return;
        }

        await audio.StopAsync(user.VoiceChannel.Guild);
        await msg.Channel.SendMessageAsync("⏹️ Stopped.");
    }
}