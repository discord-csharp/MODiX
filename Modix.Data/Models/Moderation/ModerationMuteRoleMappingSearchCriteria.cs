using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="ModerationMuteRoleMappingEntity"/> entities within an <see cref="IModerationMuteRoleMappingRepository"/>.
    /// </summary>
    public class ModerationMuteRoleMappingSearchCriteria
    {
        /// <summary>
        /// A <see cref="ModerationMuteRoleMappingEntity.GuildId"/> value, defining the <see cref="ModerationMuteRoleMappingEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// A range of values defining the <see cref="ModerationMuteRoleMappingEntity"/> entities to be returned,
        /// according to the <see cref="ConfigurationActionEntity.Created"/> value of <see cref="ModerationMuteRoleMappingEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="ModerationMuteRoleMappingEntity"/> entities to be returned.
        /// according to the <see cref="ConfigurationActionEntity.CreatedById"/> value of <see cref="ModerationMuteRoleMappingEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.DeleteActionId"/> value of null, 
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }
}
