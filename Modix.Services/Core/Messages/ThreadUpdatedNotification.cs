using Discord.WebSocket;

namespace Discord
{
    public record ThreadUpdatedNotification(Cacheable<SocketThreadChannel, ulong> OldThread, SocketThreadChannel NewThread);
}
