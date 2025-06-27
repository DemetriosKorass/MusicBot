using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    public class PlaybackOrchestrator(IYoutubeService yt, IAudioService audio) : IPlaybackOrchestrator
    {
        private static void Log(string level, string msg)
            => Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {msg}");

        public async Task AddTrackAsync(SocketSlashCommand slash, SocketGuildUser user, string videoUrl)
        {
            Log("INFO", $"User {user.Username} enqueued URL: {videoUrl}");
            var title = await yt.GetVideoTitleAsync(videoUrl) ?? "Unknown title";
            var streamUrl = await yt.GetAudioStreamUrlAsync(videoUrl)
                              ?? throw new Exception("Couldn’t get an audio stream.");

            var pending = await audio.GetQueueAsync(user.VoiceChannel.Guild);
            bool isFirst = pending.Length == 0;

            await audio.PlayAsync(user.VoiceChannel, slash.Channel, streamUrl, title);

            if (isFirst)
            {
                Log("INFO", $"Playing \"{title}\" immediately for guild {user.Guild.Id}");
                await slash.RespondAsync($"▶️ Now playing — **{title}**");
            }
            else
            {
                Log("INFO", $"Enqueued \"{title}\" at position {pending.Length + 1}");
                await slash.RespondAsync(
                    $"➡️ Enqueued **{title}**. Position: {pending.Length + 1}"
                );
            }
        }

        public async Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested PlayNext");
            await audio.SkipAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏭️ Skipped to the next track.");
        }

        public async Task StopAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested Stop");
            await audio.StopAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏹️ Playback stopped.");
        }

        public Task SkipAsync(SocketSlashCommand slash, SocketGuildUser user)
            => PlayNextAsync(slash, user);

        public async Task QueueAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested Queue");
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
            Log("INFO", $"User {user.Username} toggled loop");
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
                Log("WARN", "Stop invoked but user not in a voice channel");
                await slash.RespondAsync("❗ You must join a voice channel first.", ephemeral: true);
                return false;
            }
            return true;
        }
    }
}
