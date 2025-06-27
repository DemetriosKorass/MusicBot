using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class PlayCommandHandler(
    IPlayAction play,
    IEnqueueAction enqueue,
    IPlaybackContextManager ctxMgr
) : ICommandHandler<PlayCommand>
{
    public async Task HandleAsync(PlayCommand cmd)
    {
        await cmd.Slash.DeferAsync();

        var ctx = ctxMgr.GetOrCreate(cmd.User.VoiceChannel.Guild.Id);

        if (!ctx.IsRunning)
            await play.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url);
        else
            await enqueue.ExecuteAsync(cmd.Slash, cmd.User, cmd.Url);
    }
}
