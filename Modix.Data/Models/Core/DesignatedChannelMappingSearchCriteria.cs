using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Data.Models.Core
{
    public class DesignatedChannelMappingSearchCriteria
    {
        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.GuildId"/> value, defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.ChannelId"/> value, defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? ChannelId { get; set; }

        /// A range of values defining the <see cref="DesignatedChannelMappingEntity"/> entities to be returned,
        /// according to the <see cref="DesignatedChannelMappingEntity.Created"/> value of <see cref="DesignatedChannelMappingEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="DesignatedChannelMappingEntity.ChannelDesignation"/> value, defining the <see cref="ChannelDesignation"/> of the channels to be returned.
        /// </summary>
        public ChannelDesignation? ChannelDesignation { get; set; }

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
}
