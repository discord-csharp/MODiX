using System;
using System.Linq;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="DesignatedRoleMappingEntity"/> entities within an <see cref="IDesignatedRoleMappingRepository"/>.
    /// </summary>
    public class DesignatedRoleMappingSearchCriteria
    {
        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.GuildId"/> value, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.RoleId"/> value, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? RoleId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.Type"/> value, defining the <see cref="Type"/> of the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public DesignatedRoleType? Type { get; set; }

        /// <summary>
        /// A value defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of <see cref="DesignatedRoleMappingEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="DesignatedRoleMappingEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }

    internal static class DesignatedRoleMappingQueryableExtensions
    {
        public static IQueryable<DesignatedRoleMappingEntity> FilterBy(this IQueryable<DesignatedRoleMappingEntity> query, DesignatedRoleMappingSearchCriteria criteria)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (criteria is null)
                return query;

            var longGuildId = (long?)criteria.GuildId;
            var longRoleId = (long?)criteria.RoleId;
            var longCreatedById = (long?)criteria.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    !(longGuildId is null))
                .FilterBy(
                    x => x.RoleId == longRoleId,
                    !(longRoleId is null))
                .FilterBy(
                    x => x.Type == criteria.Type,
                    !(criteria.Type is null))
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById,
                    !(longCreatedById is null))
                .FilterBy(
                    x => x.DeleteActionId != null,
                    criteria.IsDeleted == true)
                .FilterBy(
                    x => x.DeleteActionId == null,
                    criteria.IsDeleted == false);
        }
    }
}
