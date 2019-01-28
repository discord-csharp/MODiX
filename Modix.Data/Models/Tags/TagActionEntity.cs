using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a maintenance action related to a tag.
    /// </summary>
    [Table("TagActions")]
    internal class TagActionEntity
    {
        /// <summary>
        /// The action's unique identifier.
        /// </summary>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which the action applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// A timestamp indicating when the action was performed.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The type of action that was performed.
        /// </summary>
        [Required]
        public TagActionType Type { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the user who performed the action.
        /// </summary>
        [Required]
        public ulong CreatedById { get; set; }

        /// <summary>
        /// The user who performed the action.
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; }

        /// <summary>
        /// The unique identifier of the tag that was created by the action.
        /// </summary>
        [ForeignKey(nameof(NewTag))]
        public long? NewTagId { get; set; }

        /// <summary>
        /// The tag that was created by the action.
        /// </summary>
        public virtual TagEntity NewTag { get; set; }

        /// <summary>
        /// The unique identifier of the tag that was deleted by the action.
        /// </summary>
        [ForeignKey(nameof(OldTag))]
        public long? OldTagId { get; set; }

        /// <summary>
        /// The tag that was deleted by the action.
        /// </summary>
        public virtual TagEntity OldTag { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<TagActionEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagActionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<TagActionEntity>()
                .Property(x => x.CreatedById)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagActionEntity>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });

            modelBuilder
                .Entity<TagActionEntity>()
                .HasOne(x => x.NewTag)
                .WithOne()
                .HasForeignKey<TagActionEntity>(x => x.NewTagId);

            modelBuilder
                .Entity<TagActionEntity>()
                .HasOne(x => x.OldTag)
                .WithOne()
                .HasForeignKey<TagActionEntity>(x => x.OldTagId);
        }
    }
}
