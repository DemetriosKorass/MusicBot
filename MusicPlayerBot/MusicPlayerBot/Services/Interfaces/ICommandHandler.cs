namespace MusicPlayerBot.Services.Interfaces;

public interface ICommandHandler<TCommand>
{
    Task HandleAsync(TCommand command);
}
