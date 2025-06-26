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
        if (vChannel is not SocketVoiceChannel svc)
        {
            await textChannel.SendMessageAsync("❌ I can only join guild voice channels.");
            return;
        }

        var me = svc.Guild.CurrentUser;
        var perms = me.GetPermissions(svc);
        if (!perms.Connect || !perms.Speak)
        {
            await textChannel.SendMessageAsync("❌ I need Connect & Speak permissions.");
            return;
        }

        IAudioClient client;
        try
        {
            client = await svc.ConnectAsync();
        }
        catch (TimeoutException)
        {
            await textChannel.SendMessageAsync("⚠️ Connection timed out.");
            return;
        }

        var pcm = client.CreatePCMStream(AudioApplication.Mixed);

        try
        {
            await FFMpegArguments
                .FromUrlInput(new Uri(url), options => options
                    .WithCustomArgument("-re")
                )
                .OutputToPipe(new StreamPipeSink(pcm), opts => opts
                    .WithAudioCodec("pcm_s16le")
                    .WithCustomArgument("-ar 48000")  
                    .WithCustomArgument("-ac 2")      
                    .WithCustomArgument("-f s16le")  
                )
                .ProcessAsynchronously();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioService] FFmpeg error: {ex}");
            await pcm.FlushAsync();
            await client.StopAsync();
            return;
        }

        await pcm.FlushAsync();
        await client.StopAsync();
    }

    public async Task StopAsync(IGuild guild)
    {
        if (guild is SocketGuild sg && sg.AudioClient is { } client)
            await client.StopAsync();
    }
}