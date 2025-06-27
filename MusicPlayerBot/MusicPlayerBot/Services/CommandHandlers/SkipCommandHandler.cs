using MusicPlayerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.CommandHandlers;

public class SkipCommandHandler(IAudioService audio, ILogger<SkipCommandHandler> log)
   : ICommandHandler<SkipCommand>
{
    public async Task HandleAsync(SkipCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        log.LogInformation("SkipCommand for {User}", cmd.User.Username);
        await audio.SkipAsync(cmd.User.VoiceChannel.Guild);
        await cmd.Slash.FollowupAsync("⏭️ Skipped to the next track.");
    }
}
