using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Bot.Responders;

public class AuditLogCreatedResponder :
    INotificationHandler<AuditLogCreatedNotificationV3>
{
    private readonly ModerationService _moderationService;
    private readonly IAuthorizationService _authorizationService;

    public AuditLogCreatedResponder(
        ModerationService moderationService,
        IAuthorizationService authorizationService)
    {
        _moderationService = moderationService;
        _authorizationService = authorizationService;
    }

    public async Task Handle(AuditLogCreatedNotificationV3 notification, CancellationToken cancellationToken)
    {
        if (notification.Entry.Action == ActionType.Ban && notification.Entry.Data is SocketBanAuditLogData data)
            await EnsureBanInfractionExists(notification.Guild, notification.Entry, data);
    }

    private async Task EnsureBanInfractionExists(SocketGuild guild, SocketAuditLogEntry entry, SocketBanAuditLogData data)
    {
        var bannedUser = await data.Target.GetOrDownloadAsync();

        if (await _moderationService.AnyActiveInfractions(guild.Id, bannedUser.Id, InfractionType.Ban))
            return;

        var reason = string.IsNullOrWhiteSpace(entry.Reason)
            ? $"Banned by {entry.User.GetDisplayName()}."
            : entry.Reason;

        var moderator = guild.GetUser(entry.User.Id);

        await _authorizationService.OnAuthenticatedAsync(moderator.Id, moderator.Guild.Id, moderator.Roles.Select(x => x.Id).ToList());
        await _moderationService.CreateInfractionAsync(guild.Id, entry.User.Id, InfractionType.Ban, bannedUser.Id, reason, null);
    }
}
