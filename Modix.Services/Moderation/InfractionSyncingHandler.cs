using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a handler that synchronizes infractions when applied manually through the Discord UI instead of through MODiX.
    /// </summary>
    public class InfractionSyncingHandler :
        INotificationHandler<UserBannedNotification>
    {
        private readonly IModerationService _moderationService;
        private readonly DiscordRestClient _restClient;
        private readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// Constructs a new <see cref="InfractionSyncingHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="moderationService">A moderation service to interact with the infractions system.</param>
        /// <param name="restClient">A REST client to interact with the Discord API.</param>
        public InfractionSyncingHandler(
            IModerationService moderationService,
            IAuthorizationService authorizationService,
            DiscordRestClient restClient)
        {
            _moderationService = moderationService;
            _restClient = restClient;
            _authorizationService = authorizationService;
        }

        public Task HandleNotificationAsync(UserBannedNotification notification, CancellationToken cancellationToken)
            => TryCreateBanInfractionAsync(notification.User, notification.Guild);

        /// <summary>
        /// Creates a ban infraction for the user if they do not already have one.
        /// </summary>
        /// <param name="guild">The guild that the user was banned from.</param>
        /// <param name="user">The user who was banned.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task TryCreateBanInfractionAsync(SocketUser user, SocketGuild guild)
        {
            if (await _moderationService.AnyInfractionsAsync(GetBanSearchCriteria(guild, user)))
            {
                return;
            }

            var restGuild = await _restClient.GetGuildAsync(guild.Id);
            var allAudits = await restGuild.GetAuditLogsAsync(10).FlattenAsync();

            var banLog = allAudits
                .Where(x => x.Action == ActionType.Ban && x.Data is BanAuditLogData)
                .Select(x => (Entry: x, Data: (BanAuditLogData)x.Data))
                .FirstOrDefault(x => x.Data.Target.Id == user.Id);

            // We're in a scenario in where the guild does not have a Discord audit of the
            // ban MODiX just received, if that's the case something has gone wrong and we
            // need to investigate.
            if(banLog.Data is null
               || banLog.Entry is null)
            {
                // Snapshot the most amount of information possible about this event
                // to log this incident and investigate further
                var mostRecentAudit = allAudits.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                Log.Error("No ban audit found when handling {message} for user {userId}, in guild {guild} - " +
                          "the most recent audit was created at {mostRecentAuditTime}: {mostRecentAuditAction} for user: {mostRecentAuditUserId}",
                    nameof(UserBannedNotification),
                    user.Id,
                    guild.Id,
                    mostRecentAudit?.CreatedAt,
                    mostRecentAudit?.Action,
                    mostRecentAudit?.User.Id);

                return;
            }

            var reason = string.IsNullOrWhiteSpace(banLog.Entry.Reason)
                ? $"Banned by {banLog.Entry.User.GetFullUsername()}."
                : banLog.Entry.Reason;

            var guildUser = guild.GetUser(banLog.Entry.User.Id);

            await _authorizationService.OnAuthenticatedAsync(guildUser.Id, guildUser.Guild.Id, guildUser.Roles.Select(x => x.Id).ToList());
            await _moderationService.CreateInfractionAsync(guild.Id, banLog.Entry.User.Id, InfractionType.Ban, user.Id, reason, null);
        }

        private InfractionSearchCriteria GetBanSearchCriteria(IGuild guild, IUser user)
            => new InfractionSearchCriteria()
            {
                GuildId = guild.Id,
                SubjectId = user.Id,
                Types = new[] { InfractionType.Ban },
                IsDeleted = false,
                IsRescinded = false,
            };
    }
}
