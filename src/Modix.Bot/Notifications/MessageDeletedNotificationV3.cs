using Discord;
using MediatR;

namespace Modix.Bot.Notifications;

public class MessageDeletedNotificationV3(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
    : INotification
{
    public Cacheable<IMessage, ulong> Message { get; } = message;
    public Cacheable<IMessageChannel, ulong> Channel { get; } = channel;
}
