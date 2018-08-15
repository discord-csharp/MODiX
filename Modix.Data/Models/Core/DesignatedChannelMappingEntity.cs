using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Core
{
    //This should be serialized as a string for easier
    //interop and maintainability
    public enum ChannelDesignation
    {
        ModerationLog,
        MessageLog,
        PromotionLog
    }

    public class DesignatedChannelMappingEntity
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
        public long ChannelId { get; set; }

        /// <summary>
        /// The <see cref="ChannelDesignation"/> of the channel
        /// </summary>
        [Required]
        public ChannelDesignation ChannelDesignation { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreateAction))]
        public long CreateActionID { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> that created this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        [ForeignKey(nameof(DeleteAction))]
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity DeleteAction { get; set; }
    }
}
