using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class ChannelUpdatedNotificationV3(SocketChannel oldChannel, SocketChannel newChannel) : INotification
{
    public SocketChannel OldChannel { get; } = oldChannel;
    public SocketChannel NewChannel { get; } = newChannel;
}
