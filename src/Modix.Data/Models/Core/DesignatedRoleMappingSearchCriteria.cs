using System;
using System.Collections.Generic;
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
        /// A <see cref="DesignatedRoleMappingEntity.Id"/> value, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.GuildId"/> value, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.RoleId"/> value, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? RoleId { get; set; }

        /// <summary>
        /// A set of <see cref="DesignatedRoleMappingEntity.RoleId"/> values, defining the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// A <see cref="DesignatedRoleMappingEntity"/> is considered a match if it matches any of these values.
        /// </summary>
        public ulong[]? RoleIds { get; set; }

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

            return query
                .FilterBy(
                    x => x.Id == criteria.Id,
                    !(criteria.Id is null))
                .FilterBy(
                    x => x.GuildId == criteria.GuildId,
                    !(criteria.GuildId is null))
                .FilterBy(
                    x => x.RoleId == criteria.RoleId,
                    !(criteria.RoleId is null))
                .FilterBy(
                    x => criteria.RoleIds!.Contains(x.RoleId),
                    !(criteria.RoleIds is null))
                .FilterBy(
                    x => x.Type == criteria.Type,
                    !(criteria.Type is null))
                .FilterBy(
                    x => x.CreateAction.CreatedById == criteria.CreatedById,
                    !(criteria.CreatedById is null))
                .FilterBy(
                    x => x.DeleteActionId != null,
                    criteria.IsDeleted == true)
                .FilterBy(
                    x => x.DeleteActionId == null,
                    criteria.IsDeleted == false);
        }
    }
}
