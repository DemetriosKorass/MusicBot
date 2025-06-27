using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    /// <summary>
    /// Coordinates all playback actions (Play, Stop, Skip, Queue, Loop) by
    /// calling into YouTube and Audio services and sending a single Discord response.
    /// </summary>
    public class PlaybackOrchestrator(IYoutubeService yt, IAudioService audio) : IPlaybackOrchestrator
    {
        /// <summary>
        /// Writes a timestamped log entry to the console.
        /// </summary>
        private static void Log(string level, string msg)
            => Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {msg}");

        /// <summary>
        /// Handles the Play command: resolves the video title and stream URL, 
        /// enqueues or starts playback, and sends a single response indicating
        /// whether the track is playing immediately or has been enqueued.
        /// </summary>
        /// <param name="slash">The incoming slash command context.</param>
        /// <param name="user">The guild user who invoked the command.</param>
        /// <param name="videoUrl">The YouTube video URL to play.</param>
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

        /// <summary>
        /// Signals the audio service to skip the current track and play the next one.
        /// </summary>
        /// <param name="slash">The incoming slash command context.</param>
        /// <param name="user">The guild user who invoked the command.</param>
        public async Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested PlayNext");
            await audio.SkipAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏭️ Skipped to the next track.");
        }

        /// <summary>
        /// Stops playback entirely and clears the queue.
        /// </summary>
        public async Task StopAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested Stop");
            await audio.StopAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync("⏹️ Playback stopped.");
        }

        /// <summary>
        /// Shortcut for skipping: invokes PlayNextAsync.
        /// </summary>
        public Task SkipAsync(SocketSlashCommand slash, SocketGuildUser user)
            => PlayNextAsync(slash, user);

        /// <summary>
        /// Shows the current queue of pending tracks.
        /// </summary>
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

        /// <summary>
        /// Toggles loop mode for the queue, so finished tracks are re-enqueued.
        /// </summary>
        public async Task LoopAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} toggled loop");
            var on = await audio.ToggleLoopAsync(user.VoiceChannel.Guild);
            await slash.RespondAsync(
                on ? "🔁 Repeat mode enabled." : "↪️ Repeat mode disabled.",
                ephemeral: true
            );
        }

        /// <summary>
        /// Checks whether command initiating user is in voice channel, if not - returns false.
        /// </summary>    
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
