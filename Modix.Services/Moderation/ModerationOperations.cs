using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

using Modix.Services.Core;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Describes a service for performing moderation actions, within the application, within the context of a single incoming request.
    /// </summary>
    public interface IModerationOperations
    {
        /// <summary>
        /// Creates an infraction upon a specified user, and logs an associated moderation action.
        /// </summary>
        /// <param name="type">The value to user for <see cref="InfractionEntity.Type"/>.<</param>
        /// <param name="subjectId">The value to use for <see cref="InfractionEntity.SubjectId"/>.</param>
        /// <param name="reason">The value to use for <see cref="ModerationActionEntity.Reason"/></param>
        /// <param name="duration">The value to use for <see cref="InfractionEntity.Duration"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration);

        /// <summary>
        /// Marks an existing, active, infraction of a given type, upon a given user, as rescinded.
        /// </summary>
        /// <param name="type">The <see cref="InfractionEntity.Type"/> value of the infraction to be rescinded.</param>
        /// <param name="subjectId">The <see cref="InfractionEntity.SubjectId"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(InfractionType type, ulong subjectId);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task DeleteInfractionAsync(long infractionId);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of infractions.
        /// </returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriterias = null);

        /// <summary>
        /// Retrieves a count of the types of infractions the given user has recieved.
        /// </summary>
        /// <param name="subjectId">The ID of the user to retrieve counts for</param>
        /// <returns>A <see cref="Task"/> which results in a Dictionary of infraction type to counts. Will return zeroes for types for which there are no infractions.</returns>
        Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId);
    }

    /// <inheritdoc />
    public class ModerationOperations : IModerationOperations
    {
        /// <summary>
        /// Creates a new <see cref="ModerationOperations"/>, with the given injected dependencies.
        /// </summary>
        public ModerationOperations(
            IDiscordClient discordClient,
            IModerationService moderationService,
            IAuthorizationService authorizationService,
            IUserService userService,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository,
            IInfractionRepository infractionRepository)
        {
            DiscordClient = discordClient;
            ModerationService = moderationService;
            AuthorizationService = authorizationService;
            UserService = userService;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
            InfractionRepository = infractionRepository;
        }

        /// <inheritdoc />
        public async Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(_createInfractionClaimsByType[type]);

            await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, subjectId);

            await ModerationService.CreateInfractionAsync(
                type, subjectId, reason, duration, AuthorizationService.CurrentGuildId.Value, AuthorizationService.CurrentUserId.Value);
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(InfractionType type, ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            await RequireSubjectRankLowerThanModeratorRankAsync(AuthorizationService.CurrentGuildId.Value, subjectId);

            await ModerationService.RescindInfractionAsync(
                type, subjectId, AuthorizationService.CurrentGuildId.Value, AuthorizationService.CurrentUserId.Value);
        }

        /// <inheritdoc />
        public async Task DeleteInfractionAsync(long infractionId)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationDeleteInfraction);

            var infraction = await InfractionRepository.ReadSummaryAsync(infractionId);

            await RequireSubjectRankLowerThanModeratorRankAsync(infraction.GuildId, infraction.Subject.Id);

            await ModerationService.DeleteInfractionAsync(infraction, AuthorizationService.CurrentUserId.Value);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await ModerationService.SearchInfractionsAsync(searchCriteria, sortingCriteria);
        }

        public async Task<IDictionary<InfractionType, int>> GetInfractionCountsForUserAsync(ulong subjectId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await ModerationService.GetInfractionCountsForUserAsync(subjectId, AuthorizationService.CurrentGuildId.Value);
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IModerationService"/> to be used to perform moderation operations.
        /// </summary>
        internal protected IModerationService ModerationService { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleMappingRepository"/> for storing and retrieving roles designated for use by the application.
        /// </summary>
        internal protected IDesignatedRoleMappingRepository DesignatedRoleMappingRepository { get; }

        /// <summary>
        /// An <see cref="IInfractionRepository"/> for storing and retrieving infractions designated for use by the application.
        /// </summary>
        internal protected IInfractionRepository InfractionRepository { get; }

        private async Task<IEnumerable<GuildRoleBrief>> GetRankRolesAsync()
            => (await DesignatedRoleMappingRepository
                .SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = AuthorizationService.CurrentGuildId,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(r => r.Role);

        private async Task RequireSubjectRankLowerThanModeratorRankAsync(ulong guildId, ulong subjectId)
        {
            var moderator = await UserService.GetGuildUserAsync(guildId, AuthorizationService.CurrentUserId.Value);

            if (moderator.GuildPermissions.Administrator)
                return;

            var rankRoles = await GetRankRolesAsync();

            var subject = await UserService.GetGuildUserAsync(guildId, subjectId);

            var subjectRankRoles = rankRoles.Where(r => subject.RoleIds.Contains(r.Id));
            var moderatorRankRoles = rankRoles.Where(r => moderator.RoleIds.Contains(r.Id));

            var greatestSubjectRankPosition = subjectRankRoles.Any()
                ? subjectRankRoles.Select(r => r.Position).Max()
                : default(int?);
            var greatestModeratorRankPosition = moderatorRankRoles.Any()
                ? moderatorRankRoles.Select(r => r.Position).Max()
                : default(int?);

            if (greatestModeratorRankPosition is null
                || greatestSubjectRankPosition >= greatestModeratorRankPosition)
            {
                throw new InvalidOperationException("Cannot moderate users that have a rank greater than or equal to your own.");
            }
        }

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
            = new Dictionary<InfractionType, AuthorizationClaim>()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote },
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn },
                {InfractionType.Mute, AuthorizationClaim.ModerationMute },
                {InfractionType.Ban, AuthorizationClaim.ModerationBan }
            };
    }
}
