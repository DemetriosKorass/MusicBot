using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.CommandHandlers;

public class SkipCommandHandler(
    ISkipAction skip
) : ICommandHandler<SkipCommand>
{
    public async Task HandleAsync(SkipCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        await skip.ExecuteAsync(
            cmd.Slash,
            cmd.User
        );
    }
}
