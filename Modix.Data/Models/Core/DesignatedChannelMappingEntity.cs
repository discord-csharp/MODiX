using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Core
{
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
        /// The <see cref="GuildChannelEntity.ChannelId"/> value of <see cref="Channel"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Channel))]
        public long ChannelId { get; set; }

        /// <summary>
        /// The channel to which this mapping applies.
        /// </summary>
        public virtual GuildChannelEntity Channel { get; set; }

        /// <summary>
        /// The type of designation being mapped to <see cref="Channel"/>.
        /// </summary>
        [Required]
        public DesignatedChannelType Type { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> that created this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity DeleteAction { get; set; }
    }
}
