using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class UserJoinedNotificationV3(SocketGuildUser guildUser) : INotification
{
    public SocketGuildUser GuildUser { get; } = guildUser;
}
