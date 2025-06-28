namespace MusicPlayerBot.Data;

public sealed record Track
{
    /// <summary>The YouTube video ID (11-char) or other unique identifier.</summary>
    public required string Id { get; init; }

    /// <summary>The fully-qualified stream URL (HLS or direct)</summary>
    public required string StreamUrl { get; init; }

    /// <summary>The human-readable title.</summary>
    public required string Title { get; init; }

    /// <summary>Optional total duration, if known.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Optional thumbnail URL.</summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>Factory: create a Track with only URL & title.</summary>
    public static Track FromInfo(string id, string url, string title)
        => new() { Id = id, StreamUrl = url, Title = title };

    /// <summary>Format for display in chat.</summary>
    public string DisplayName
        => $"**{Title}**{(Duration.HasValue ? $" ({Duration:hh\\:mm\\:ss})" : "")}";

    private Track() { }  
}
