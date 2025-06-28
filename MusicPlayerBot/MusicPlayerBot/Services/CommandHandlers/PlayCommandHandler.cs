using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class PlayCommandHandler(
   IAddTrackAction add,
   IEnqueueAction enqueue,
   ICheckVoiceAction checkVoice
) : ICommandHandler<PlayCommand>
{
    public async Task HandleAsync(PlayCommand cmd, PlaybackContext ctx)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();

        if (!ctx.IsRunning)
            await add.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url, ctx);
        else
            await enqueue.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url, ctx);
    }
}
