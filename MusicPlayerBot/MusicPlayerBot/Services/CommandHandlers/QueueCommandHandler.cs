using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class QueueCommandHandler(
    IShowQueueAction queue,
    ICheckVoiceAction checkVoice
) : ICommandHandler<QueueCommand>
{
    public async Task HandleAsync(QueueCommand cmd)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();
        await queue.ExecuteAsync(cmd.Slash, cmd.User);
    }
}
