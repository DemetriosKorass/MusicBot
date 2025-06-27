using Discord.WebSocket;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services
{
    /// <summary>
    /// Coordinates playback operations: resolves YouTube URLs, enqueues or plays tracks,
    /// and sends unified responses for the Play slash command.
    /// </summary>
    public class PlaybackOrchestrator(IYoutubeService yt, IAudioService audio) : IPlaybackOrchestrator
    {
        /// <summary>
        /// Writes a timestamped log entry to the console.
        /// </summary>
        /// <param name="level">The log level (e.g. INFO, WARN).</param>
        /// <param name="msg">The message to log.</param>
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

            Log("INFO", isFirst
                ? $"Playing track \"{title}\" for guild {user.Guild.Id}"
                : $"Enqueued track \"{title}\" for guild {user.Guild.Id}"
            );

            if (isFirst)
                await slash.RespondAsync($"▶️ Now playing — **{title}**");
            else
                await slash.RespondAsync($"➡️ Enqueued **{title}**. Position: {pending.Length + 1}");
        }

        /// <summary>
        /// Signals the audio service to skip the current track and start the next one.
        /// Sends a single acknowledgment response.
        /// </summary>
        /// <param name="slash">The incoming slash command context.</param>
        /// <param name="user">The guild user who invoked the command.</param>
        public async Task PlayNextAsync(SocketSlashCommand slash, SocketGuildUser user)
        {
            Log("INFO", $"User {user.Username} requested PlayNext in guild {user.Guild.Id}");
            await audio.SkipAsync(user.VoiceChannel.Guild);
            Log("INFO", "Signaled AudioService to play next track");
            await slash.RespondAsync("▶️ **Playing next track**…");
        }
    }
}
