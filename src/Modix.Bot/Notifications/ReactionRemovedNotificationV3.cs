using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class ReactionRemovedNotificationV3(
    Cacheable<IUserMessage, ulong> message,
    Cacheable<IMessageChannel, ulong> channel,
    SocketReaction reaction): INotification
{
    public Cacheable<IUserMessage, ulong> Message { get; } = message;
    public Cacheable<IMessageChannel, ulong> Channel { get; } = channel;
    public SocketReaction Reaction { get; } = reaction;
}
