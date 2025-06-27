using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class PlayCommandHandler(
    IPlayAction play,
    IEnqueueAction enqueue,
    ICheckVoiceAction checkVoice,
    IPlaybackContextManager ctxMgr
) : ICommandHandler<PlayCommand>
{
    public async Task HandleAsync(PlayCommand cmd)
    {
        if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
            return;

        await cmd.Slash.DeferAsync();

        var ctx = ctxMgr.GetOrCreate(cmd.User.Guild.Id);

        if (!ctx.IsRunning)
            await play.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url);
        else
            await enqueue.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url);
    }
}
