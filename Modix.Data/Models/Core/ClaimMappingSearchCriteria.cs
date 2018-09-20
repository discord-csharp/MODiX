using System.Collections.Generic;
using System.Linq;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ClaimMappingEntity"/> entities within an <see cref="IClaimMappingRepository"/>.
    /// </summary>
    public class ClaimMappingSearchCriteria
    {
        /// <summary>
        /// A set of <see cref="ClaimMappingEntity.Type"/> values, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<ClaimMappingType> Types { get; set; }

        /// <summary>
        /// A <see cref="ClaimMappingEntity.GuildId"/> value, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A set of <see cref="ClaimMappingEntity.RoleId"/> values, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<ulong> RoleIds { get; set; }

        /// <summary>
        /// A <see cref="ClaimMappingEntity.UserId"/> value, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// A set of <see cref="ClaimMappingEntity.Claim"/> values, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<AuthorizationClaim> Claims { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="ClaimMappingEntity"/> entities to be returned,
        /// according to the <see cref="ConfigurationActionEntity.Created"/> value of <see cref="ClaimMappingEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="ClaimMappingEntity"/> entities to be returned,
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of <see cref="ClaimMappingEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="ClaimMappingEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }

    internal static class ClaimMappingQueryableExtensions
    {
        internal static IQueryable<ClaimMappingEntity> FilterBy(this IQueryable<ClaimMappingEntity> query, ClaimMappingSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longRoleIds = criteria?.RoleIds?.Select(x => (long)x).ToArray();
            var longUserId = (long?)criteria?.UserId;
            var longCreatedById = (long?)criteria?.CreatedById;

            var anyRoleIds = longRoleIds?.Any() ?? false;

            return query
                .FilterBy(
                    x => criteria.Types.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => longRoleIds.Contains(x.RoleId.Value) || (x.UserId == longUserId),
                    anyRoleIds && (longUserId != null))
                .FilterBy(
                    x => longRoleIds.Contains(x.RoleId.Value),
                    anyRoleIds && (longUserId == null))
                .FilterBy(
                    x => (x.UserId == longUserId),
                    !anyRoleIds && (longUserId != null))
                .FilterBy(
                    x => criteria.Claims.Contains(x.Claim),
                    criteria?.Claims?.Any() ?? false)
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange.Value.From.Value,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange.Value.To.Value,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById,
                    longCreatedById != null)
                .FilterBy(
                    x => (x.DeleteActionId != null) == criteria.IsDeleted.Value,
                    criteria?.IsDeleted != null);
        }
    }
}
