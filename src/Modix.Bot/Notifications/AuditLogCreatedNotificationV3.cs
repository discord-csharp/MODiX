using Discord.WebSocket;
using MediatR;

namespace Modix.Bot.Notifications;

public class AuditLogCreatedNotificationV3(SocketAuditLogEntry entry, SocketGuild guild) : INotification
{
    public SocketAuditLogEntry Entry { get; } = entry;
    public SocketGuild Guild { get; } = guild;
}
