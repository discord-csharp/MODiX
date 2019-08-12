using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes the result of one or more moderation actions performed upon a user, by a staff member.
    /// </summary>
    public class InfractionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="InfractionEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this infraction applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The type of <see cref="InfractionEntity"/> recorded.
        /// </summary>
        [Required]
        public InfractionType Type { get; set; }

        /// <summary>
        /// A comment about why the infraction was recorded.
        /// </summary>
        [Required]
        public string Reason { get; set; }

        /// <summary>
        /// The duration from <see cref="Created"/>, indicating when the infraction should be considered "expired".
        /// A null value indicates that the action does not expire.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="Subject"/>.
        /// </summary>
        [Required]
        public ulong SubjectId { get; set; }

        /// <summary>
        /// The user upon which the <see cref="InfractionEntity"/> was applied.
        /// </summary>
        [Required]
        public virtual GuildUserEntity Subject { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> that created this <see cref="InfractionEntity"/>.
        /// </summary>
        [Required]
        public virtual ModerationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="RescindAction"/>.
        /// </summary>
        public long? RescindActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> (if any) that rescinded this <see cref="InfractionEntity"/>.
        /// </summary>
        public virtual ModerationActionEntity RescindAction { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> (if any) that deleted this <see cref="InfractionEntity"/>.
        /// </summary>
        public virtual ModerationActionEntity DeleteAction { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="UpdateAction"/>.
        /// </summary>
        public long? UpdateActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> (if any) that updated this <see cref="InfractionEntity"/>.
        /// </summary>
        public virtual ModerationActionEntity UpdateAction { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.SubjectId)
                .HasConversion<long>();

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.CreateActionId);

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.RescindAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.RescindActionId);

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.DeleteActionId);

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.UpdateAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.UpdateActionId);
        }
    }
}
