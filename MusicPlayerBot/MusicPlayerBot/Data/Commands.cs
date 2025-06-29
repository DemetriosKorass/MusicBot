using Discord.WebSocket;

namespace MusicPlayerBot.Data;

public record PlayCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User,
    string Url
);
public record StopCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User
);
public record SkipCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User
);
public record QueueCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User
);
public record LoopCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User
);
public record DisposeCommand(
    SocketSlashCommand Slash,
    SocketGuildUser User
);