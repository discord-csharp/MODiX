using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationLogChannelMappingEntity"/> entities within an <see cref="IModerationLogChannelMappingRepository"/>.
    /// </summary>
    public class ModerationLogChannelMappingSearchCriteria
    {
        /// <summary>
        /// A <see cref="ModerationLogChannelMappingEntity.GuildId"/> value, defining the <see cref="ModerationLogChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="ModerationLogChannelMappingEntity.LogChannelId"/> value, defining the <see cref="ModerationLogChannelMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? LogChannelId { get; set; }

        /// A range of values defining the <see cref="ModerationLogChannelMappingEntity"/> entities to be returned,
        /// according to the <see cref="ConfigurationActionEntity.Created"/> value of <see cref="ModerationLogChannelMappingEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="ModerationLogChannelMappingEntity"/> entities to be returned.
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of <see cref="ModerationLogChannelMappingEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}
