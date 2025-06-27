using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class StopCommandHandler(
    IStopAction stop,
    ICheckVoiceAction checkVoice
) : ICommandHandler<StopCommand>
{
    public async Task HandleAsync(StopCommand cmd)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();
        await stop.ExecuteAsync(cmd.Slash, cmd.User);
    }
}
