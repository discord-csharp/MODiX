using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class MessageUpdatedNotificationV3(Cacheable<IMessage, ulong> cachedMessage, SocketMessage newMessage, ISocketMessageChannel channel) : INotification
{
    public Cacheable<IMessage, ulong> Cached { get; } = cachedMessage;
    public SocketMessage Message { get; } = newMessage;
    public ISocketMessageChannel Channel { get; } = channel;
}
