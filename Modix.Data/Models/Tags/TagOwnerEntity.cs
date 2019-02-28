using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes the owner of a tag.
    /// </summary>
    [Table("TagOwners")]
    internal class TagOwnerEntity
    {
        /// <summary>
        /// The unique identifier of the tag owner record.
        /// </summary>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which the tag belongs.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the User to whom the tag belongs, if any.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// The user to whom the tag belongs, if any.
        /// </summary>
        public virtual GuildUserEntity User { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the role to which the tag belongs, if any.
        /// </summary>
        public ulong? RoleId { get; set; }

        /// <summary>
        /// The role to which the tag belongs, if any.
        /// </summary>
        public virtual GuildRoleEntity Role { get; set; }

        /// <summary>
        /// The name of the tag that the owner owns.
        /// </summary>
        [Required]
        public string TagName { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<TagOwnerEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagOwnerEntity>()
                .Property(x => x.UserId)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagOwnerEntity>()
                .Property(x => x.RoleId)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.UserId });

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId);

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasIndex(x => x.GuildId);

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasIndex(x => x.UserId);

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasIndex(x => x.RoleId);

            modelBuilder
                .Entity<TagOwnerEntity>()
                .HasIndex(x => x.TagName)
                .IsUnique();
        }
    }
}
