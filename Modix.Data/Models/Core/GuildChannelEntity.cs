using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a message channel, within a Discord guild managed by the application,
    /// which may or may not still exist.
    /// </summary>
    public class GuildChannelEntity
    {
        /// <summary>
        /// The snowflake ID of the channel, within the Discord API.
        /// </summary>
        [Key]
        [Required]
        public long ChannelId { get; set; }

        /// <summary>
        /// The snowflake ID of the guild to which this channel belongs.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// The last-known name of the channel.
        /// </summary>
        [Required]
        public string Name { get; set; }
    }

}
