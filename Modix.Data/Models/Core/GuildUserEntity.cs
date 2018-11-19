using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a user, that is tracked on a per-guild basis within the application.
    /// </summary>
    public class GuildUserEntity
    {
        /// <summary>
        /// The Discord snowflake ID of the guild for which data is tracked by this entity.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="UserEntity.Id"/> value of <see cref="User"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(User))]
        public ulong UserId { get; set; }

        /// <summary>
        /// The user whose data is tracked by this entity.
        /// </summary>
        [Required]
        public virtual UserEntity User { get; set; }

        /// <summary>
        /// The Discord Nickname value of the user, within the guild.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// A timestamp indicating the first time this user was observed within the guild.
        /// </summary>
        [Required]
        public DateTimeOffset FirstSeen { get; set; }

        /// <summary>
        /// A timestamp indicating the most recent time this user was observed within the guild.
        /// </summary>
        [Required]
        public DateTimeOffset LastSeen { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<GuildUserEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<GuildUserEntity>()
                .Property(x => x.UserId)
                .HasConversion<long>();

            modelBuilder
                .Entity<GuildUserEntity>()
                .HasKey(x => new { x.GuildId, x.UserId });
        }
    }
}
