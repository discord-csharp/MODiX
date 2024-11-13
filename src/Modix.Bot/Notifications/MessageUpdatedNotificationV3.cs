using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class MessageUpdatedNotificationV3(
    Cacheable<IMessage, ulong> oldMessage,
    SocketMessage newMessage,
    ISocketMessageChannel channel) : INotification
{
    public Cacheable<IMessage, ulong> OldMessage { get; } = oldMessage;
    public SocketMessage NewMessage { get; } = newMessage;
    public ISocketMessageChannel Channel { get; } = channel;
}
