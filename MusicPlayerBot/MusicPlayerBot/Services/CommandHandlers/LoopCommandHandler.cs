using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class LoopCommandHandler(
    IEnableLoopAction loop,
    ICheckVoiceAction checkVoice
) : ICommandHandler<LoopCommand>
{
    public async Task HandleAsync(LoopCommand cmd)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();
        await loop.ExecuteAsync(cmd.Slash, cmd.User);
    }
}
