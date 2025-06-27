using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class SkipCommandHandler(
    ISkipAction skip,
    ICheckVoiceAction checkVoice
) : ICommandHandler<SkipCommand>
{
    public async Task HandleAsync(SkipCommand cmd)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();
        await skip.ExecuteAsync(cmd.Slash, cmd.User);
    }
}
