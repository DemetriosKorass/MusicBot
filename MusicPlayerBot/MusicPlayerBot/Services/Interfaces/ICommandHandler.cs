using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Defines a handler for processing commands in the music player bot.
/// </summary>
/// <typeparam name="TCommand">The type of the command to be handled.</typeparam>
public interface ICommandHandler<TCommand>
{
    /// <summary>
    /// Handles the specified command asynchronously within the given playback context.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="context">The playback context in which the command is executed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TCommand command, PlaybackContext context);
}
