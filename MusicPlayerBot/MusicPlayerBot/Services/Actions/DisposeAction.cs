using Discord.WebSocket;
using MusicPlayerBot.Data;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Actions
{
    public class DisposeAction : IDisposeAction
    {
        public async Task ExecuteAsync(SocketSlashCommand slash, SocketGuildUser user, PlaybackContext ctx)
        {
            if (ctx.VoiceChannel != null)
            {
                await ctx.VoiceChannel.DisconnectAsync();
            }
        }
    }
}
