using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Mentions
{
    /// <summary>
    /// Describes the configured rules for mentioning a role.
    /// </summary>
    [Table("MentionMappings")]
    public class MentionMappingEntity
    {
        /// <summary>
        /// The Discord snowflake ID of the role to which this mapping applies.
        /// </summary>
        [Key]
        [Required]
        [ForeignKey(nameof(Role))]
        public ulong RoleId { get; set; }

        /// <summary>
        /// The role to which this mapping applies.
        /// </summary>
        [Required]
        public GuildRoleEntity Role { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// Indicates whether the role is mentionable and, if so, through what method.
        /// </summary>
        [Required]
        public MentionabilityType Mentionability { get; set; }

        /// <summary>
        /// The unique identifier of the minimum rank role, if any, required to mention the role.
        /// </summary>
        [ForeignKey(nameof(MinimumRank))]
        public ulong? MinimumRankId { get; set; }

        /// <summary>
        /// The minimum rank role, if any, required to mention the role.
        /// </summary>
        public GuildRoleEntity MinimumRank { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<MentionMappingEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<MentionMappingEntity>()
                .Property(x => x.RoleId)
                .HasConversion<long>();

            modelBuilder
                .Entity<MentionMappingEntity>()
                .Property(x => x.Mentionability)
                .HasConversion<string>();

            modelBuilder
                .Entity<MentionMappingEntity>()
                .HasOne(x => x.Role)
                .WithOne()
                .HasForeignKey<MentionMappingEntity>(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<MentionMappingEntity>()
                .HasOne(x => x.MinimumRank)
                .WithMany()
                .HasForeignKey(x => x.MinimumRankId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
