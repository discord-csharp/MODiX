using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class MessageReceivedNotificationV3(SocketMessage message) : INotification
{
    public IMessage Message { get; } = message;
}
