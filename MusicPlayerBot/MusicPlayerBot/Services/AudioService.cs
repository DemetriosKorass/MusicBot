using Discord;
using Discord.Audio;
using Discord.WebSocket;
using FFMpegCore;
using FFMpegCore.Pipes;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services;
public class AudioService : IAudioService
{
    public async Task PlayAsync(IVoiceChannel vChannel, IMessageChannel textChannel, string url)
    {
        // Connect to voice
        var conn = await vChannel.ConnectAsync();
        var pcm = conn.CreatePCMStream(AudioApplication.Mixed);

        // Fire-and-forget ffmpeg → discord pipe:
        _ = FFMpegArguments
            .FromUrlInput(new Uri(url))
            .OutputToPipe(new StreamPipeSink(pcm), opts => opts
                .WithAudioCodec("pcm_s16le")
                .WithCustomArgument("-ar 48000")     // sample rate
                .WithCustomArgument("-ac 2")         // channels
                .WithCustomArgument("-f s16le")      // format
            )
            .ProcessAsynchronously()
            .ContinueWith(t =>
            {
                // when ffmpeg exits, flush and disconnect
                pcm.FlushAsync().GetAwaiter().GetResult();
                conn.StopAsync().GetAwaiter().GetResult();
            });

        await textChannel.SendMessageAsync("▶️ Playing now!");
    }

    public async Task StopAsync(IGuild guild)
    {
        var conn = (guild as SocketGuild)?.AudioClient;
        if (conn != null)
            await conn.StopAsync();
    }
}