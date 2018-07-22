using System.Collections.Generic;

using Modix.Data.Repositories;

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
        /// A set of <see cref="ClaimMappingEntity.Type"/> values, defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// </summary>
        public IReadOnlyCollection<AuthorizationClaim> Claims { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="ClaimMappingEntity"/> entities to be returned,
        /// according to the <see cref="ConfigurationActionEntity.Created"/> value of associated <see cref="ConfigurationActionEntity"/> entities,
        /// with a <see cref="ConfigurationActionEntity.Type"/> value of <see cref="ConfigurationActionType.ClaimMappingCreated"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="ClaimMappingEntity"/> entities to be returned.
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of associated <see cref="ConfigurationActionEntity"/> entities,
        /// with a <see cref="ConfigurationActionEntity.Type"/> value of <see cref="ConfigurationActionType.ClaimMappingCreated"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="ClaimMappingEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}
