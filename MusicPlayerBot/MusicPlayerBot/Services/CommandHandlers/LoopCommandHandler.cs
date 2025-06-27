using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class LoopCommandHandler(
    ILoopAction loop
) : ICommandHandler<LoopCommand>
{
    public async Task HandleAsync(LoopCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        await loop.ExecuteAsync(
            cmd.User.VoiceChannel.Guild,
            cmd.Slash.Channel
        );
    }
}
