using MusicPlayerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.CommandHandlers;

public class LoopCommandHandler(
    IAudioService audio,
    ILogger<LoopCommandHandler> log
    ) : ICommandHandler<LoopCommand>
{
    public async Task HandleAsync(LoopCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        log.LogInformation("LoopCommand for {User}", cmd.User.Username);
        var on = await audio.ToggleLoopAsync(cmd.User.VoiceChannel.Guild);
        await cmd.Slash.FollowupAsync(
            on ? "🔁 Repeat mode enabled." : "↪️ Repeat mode disabled.",
            ephemeral: true
        );
    }
}