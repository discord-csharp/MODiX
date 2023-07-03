using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a handler that synchronizes infractions when applied manually through the Discord UI instead of through MODiX.
    /// </summary>
    public class InfractionSyncingHandler :
        INotificationHandler<AuditLogCreatedNotification>
    {
        private readonly IModerationService _moderationService;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Constructs a new <see cref="InfractionSyncingHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="moderationService">A moderation service to interact with the infractions system.</param>
        /// <param name="restClient">A REST client to interact with the Discord API.</param>
        public InfractionSyncingHandler(
            IModerationService moderationService,
            IAuthorizationService authorizationService)
        {
            _moderationService = moderationService;
            _authorizationService = authorizationService;
        }

        public async Task HandleNotificationAsync(AuditLogCreatedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Entry.Action == ActionType.Ban && notification.Entry.Data is SocketBanAuditLogData data)
                await TryCreateBanInfractionAsync(notification.Guild, notification.Entry, data);
        }

        private async Task TryCreateBanInfractionAsync(SocketGuild guild, SocketAuditLogEntry entry, SocketBanAuditLogData data)
        {
            var bannedUser = await data.Target.GetOrDownloadAsync();

            if (await _moderationService.AnyInfractionsAsync(GetBanSearchCriteria(guild, bannedUser)))
                return;

            var reason = string.IsNullOrWhiteSpace(entry.Reason)
                ? $"Banned by {entry.User.GetDisplayName()}."
                : entry.Reason;

            var moderator = guild.GetUser(entry.User.Id);

            await _authorizationService.OnAuthenticatedAsync(moderator.Id, moderator.Guild.Id, moderator.Roles.Select(x => x.Id).ToList());
            await _moderationService.CreateInfractionAsync(guild.Id, entry.User.Id, InfractionType.Ban, bannedUser.Id, reason, null);
        }

        private static InfractionSearchCriteria GetBanSearchCriteria(IGuild guild, IUser user)
            => new()
            {
                GuildId = guild.Id,
                SubjectId = user.Id,
                Types = new[] { InfractionType.Ban },
                IsDeleted = false,
                IsRescinded = false,
            };
    }
}
