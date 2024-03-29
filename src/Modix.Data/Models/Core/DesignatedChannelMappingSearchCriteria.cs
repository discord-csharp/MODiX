using System;
using System.Linq;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="DesignatedRoleMappingEntity"/> entities within an <see cref="IDesignatedChannelMappingRepository"/>.
    /// </summary>
    public class DesignatedChannelMappingSearchCriteria
    {
        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.Id"/> value, defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.GuildId"/> value, defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.ChannelId"/> value, defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? ChannelId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedRoleMappingEntity.Type"/> value, defining the <see cref="Type"/> of the <see cref="DesignatedRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public DesignatedChannelType? Type { get; set; }

        /// <summary>
        /// A value defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of <see cref="DesignatedChannelMappingEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }

    internal static class DesignatedChannelMappingQueryableExtensions
    {
        public static IQueryable<DesignatedChannelMappingEntity> FilterBy(this IQueryable<DesignatedChannelMappingEntity> query, DesignatedChannelMappingSearchCriteria criteria)
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
                    x => x.ChannelId == criteria.ChannelId,
                    !(criteria.ChannelId is null))
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
