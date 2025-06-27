using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class StopCommandHandler(
    IStopAction stop
) : ICommandHandler<StopCommand>
{
    public async Task HandleAsync(StopCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        await stop.ExecuteAsync(
            cmd.User.VoiceChannel.Guild,
            cmd.Slash.Channel
        );
    }
}
