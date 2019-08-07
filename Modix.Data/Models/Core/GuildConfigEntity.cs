using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a Discord guild, tracked by the application. 
    /// This configuration item stores guild-specific configurations for one off items.
    /// </summary>
    public class GuildConfigEntity
    {
        /// <summary>
        /// The Discord snowflake ID of the guild which this config controls.
        /// </summary>
        [Key]
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// Represents a minimum number of days after a user joins the guild after which
        /// they may be promoted.
        /// </summary>
        /// <remarks>Set this value to 0 to disable.</remarks>
        public int MinimumDaysBeforePromotion { get; set; } = 20;
    }
}
