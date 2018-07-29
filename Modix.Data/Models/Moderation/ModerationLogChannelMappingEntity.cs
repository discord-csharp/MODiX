using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a configuration setting for a guild, that defines the Discord Channel to which moderation actions should be logged.
    /// </summary>
    public class ModerationLogChannelMappingEntity
    {
        /// <summary>
        /// A unique identifier for this mapping.
        /// </summary>
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the channel to receive logging messages.
        /// </summary>
        [Required]
        public long LogChannelId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreateAction))]
        public long CreateActionID { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> that created this <see cref="ModerationLogChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        [ForeignKey(nameof(DeleteAction))]
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="ModerationLogChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity DeleteAction { get; set; }
    }
}
