
namespace MusicPlayerBot.Services.Interfaces;

/// <summary>
/// Encodes a remote input (URL or file) into raw PCM and writes it to the provided stream.
/// </summary>
public interface IAudioEncoder
{
    Task EncodeToPcmAsync(string input, Stream output, CancellationToken cancelToken);
}
