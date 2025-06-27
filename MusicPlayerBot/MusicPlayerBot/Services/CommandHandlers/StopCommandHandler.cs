using MusicPlayerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.CommandHandlers;

public class StopCommandHandler(
    IAudioService audio,
    ILogger<StopCommandHandler> log
    ) : ICommandHandler<StopCommand>
{
    public async Task HandleAsync(StopCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        log.LogInformation("StopCommand for {User}", cmd.User.Username);
        await audio.StopAsync(cmd.User.VoiceChannel.Guild);
        await cmd.Slash.FollowupAsync("⏹️ Playback stopped.");
    }
}