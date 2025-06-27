using MusicPlayerBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Data;

namespace MusicPlayerBot.Services.CommandHandlers;

public class QueueCommandHandler(
    IAudioService audio,
    ILogger<QueueCommandHandler> log
    ) : ICommandHandler<QueueCommand>
{
    public async Task HandleAsync(QueueCommand cmd)
    {
        await cmd.Slash.DeferAsync();
        log.LogInformation("QueueCommand for {User}", cmd.User.Username);
        var items = await audio.GetQueueAsync(cmd.User.VoiceChannel.Guild);
        if (items.Length == 0)
            await cmd.Slash.FollowupAsync("📃 The queue is empty.", ephemeral: true);
        else
            await cmd.Slash.FollowupAsync(
                "📃 **Upcoming tracks:**\n" +
                string.Join("\n", items.Select((t, i) => $"{i + 1}. {t}")),
                ephemeral: true
            );
    }
}
