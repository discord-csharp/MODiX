using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class ChannelCreatedNotificationV3(SocketChannel channel) : INotification
{
    public SocketChannel Channel { get; } = channel;
}
