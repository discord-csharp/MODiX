using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The snowflake ID of the guild to which this channel belongs.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The last-known name of the channel.
        /// </summary>
        [Required]
        public string Name { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<GuildChannelEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder
                .Entity<GuildChannelEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();
        }
    }
}
