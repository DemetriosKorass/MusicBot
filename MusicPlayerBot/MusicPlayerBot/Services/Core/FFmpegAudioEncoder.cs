using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;

public class FFmpegAudioEncoder(ILogger<FFmpegAudioEncoder> logger) : IAudioEncoder
{
    public async Task EncodeToPcmAsync(string input, Stream output, CancellationToken cancelToken)
    {
        var args = $"-re -i \"{input}\" -ac 2 -f s16le -ar 48000 pipe:1";
        var psi = new ProcessStartInfo("ffmpeg", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi)!;

        _ = Task.Run(async () =>
        {
            while (!proc.HasExited)
            {
                var line = await proc.StandardError.ReadLineAsync();
                if (line != null) logger.LogDebug("FFmpeg output: {Line}", line);
            }
        }, cancelToken);

        await proc.StandardOutput.BaseStream.CopyToAsync(output, cancelToken);
        proc.WaitForExit();

        if (proc.ExitCode != 0)
            logger.LogWarning("FFmpeg exited with code {Code}", proc.ExitCode);
    }
}
