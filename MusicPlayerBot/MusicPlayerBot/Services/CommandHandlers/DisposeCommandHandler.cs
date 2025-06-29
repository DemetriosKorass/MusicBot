using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers
{
    public class DisposeCommandHandler(
        IStopAction stopAction,
        IDisposeAction dispose,
        ICheckVoiceAction checkVoice)
        : ICommandHandler<DisposeCommand> 
    {
        public async Task HandleAsync(DisposeCommand cmd, PlaybackContext ctx)
        {
            if (!await checkVoice.ExecuteAsync(cmd.Slash, cmd.User))
                return;

            await cmd.Slash.DeferAsync();
            await stopAction.ExecuteAsync(cmd.Slash, cmd.User, ctx);
            await dispose.ExecuteAsync(cmd.Slash, cmd.User, ctx);
        }
    }
}
