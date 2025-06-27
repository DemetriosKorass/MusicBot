using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class QueueCommandHandler(
    IQueueAction queue
) : ICommandHandler<QueueCommand>
{
    public async Task HandleAsync(QueueCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        await queue.ExecuteAsync(
            cmd.User.VoiceChannel.Guild,
            cmd.Slash.Channel
        );
    }
}
