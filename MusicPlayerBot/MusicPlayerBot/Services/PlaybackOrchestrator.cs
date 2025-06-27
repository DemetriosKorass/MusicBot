using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    public class PlaybackOrchestrator(IYoutubeService yt, IAudioService audio, ILogger<PlaybackOrchestrator> logger) : IPlaybackOrchestrator
    {
        public async Task AddTrackAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl)
        {
            logger.LogInformation("User {Username} enqueued URL: {VideoUrl}", user.Username, videoUrl);
            var title = await yt.GetVideoTitleAsync(videoUrl) ?? "Unknown title";
            var streamUrl = await yt.GetAudioStreamUrlAsync(videoUrl)
                              ?? throw new Exception("Couldn’t get an audio stream.");

            var wasPlaying = await audio.IsPlayingAsync(user.VoiceChannel.Guild);
            bool isFirst = !wasPlaying;

            await audio.PlayAsync(user.VoiceChannel, slash.Channel, streamUrl, title);

            if (isFirst)
            {
                logger.LogInformation("Playing \"{Title}\" immediately for guild {GuildId}", title, user.Guild.Id);
                await slash.RespondAsync($"▶️ Now playing — **{title}**");
            }
            else
            {
                var queue = await audio.GetQueueAsync(user.VoiceChannel.Guild);
                logger.LogInformation("Enqueued \"{Title}\" at position {QueueLength}", title, queue.Length);
                await slash.RespondAsync(
                   $"➡️ Enqueued **{title}**. Position: {queue.Length}"
                );
            }
        }

        public async Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            logger.LogInformation("User {Username} requested PlayNext", user.Username);
            await audio.SkipAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏭️ Skipped to the next track.");
        }

        public async Task StopAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            logger.LogInformation("User {Username} requested Stop", user.Username);
            await audio.StopAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏹️ Playback stopped.");
        }

        public Task SkipAsync(SocketSlashCommand slash, SocketGuildUser user)
            => PlayNextAsync(slash, user);

        public async Task QueueAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            logger.LogInformation("User {Username} requested Queue", user.Username);
            var items = await audio.GetQueueAsync(user.VoiceChannel.Guild);
            if (items.Length == 0)
                await slash.RespondAsync("📃 The queue is empty.", ephemeral: true);
            else
                await slash.RespondAsync(
                    "📃 **Upcoming tracks:**\n" + string.Join("\n", items.Select((t, i) => $"{i + 1}. {t}")),
                    ephemeral: true
                );
        }

        public async Task LoopAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            logger.LogInformation("User {Username} toggled loop", user.Username);
            var on = await audio.ToggleLoopAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync(
                on ? "🔁 Repeat mode enabled." : "↪️ Repeat mode disabled.",
                ephemeral: true
            );
        }

        public async Task<bool> CheckIfUserIsInChannelAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            if (user?.VoiceChannel == null)
            {
                logger.LogWarning("Command invoked but user not in a voice channel");
                await slash.RespondAsync("❗ You must join a voice channel first.", ephemeral: true);
                return false;
            }
            return true;
        }
    }
}
