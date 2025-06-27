using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions;

/// <inheritdoc cref="ICheckVoiceAction"/>  
public class CheckVoiceAction(ILogger<CheckVoiceAction> logger)
   : ICheckVoiceAction
{
    public async Task<bool> ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user)
    {
        if (user.VoiceChannel == null)
        {
            logger.LogWarning(
                "User {Username} attempted a command without joining a voice channel",
                user.Username
            );
            await slash.RespondAsync(
                "❗ You must join a voice channel first.",
                ephemeral: true
            );
            return false;
        }

        return true;
    }
}
