using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    public class InfractionEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ulong GuildId { get; set; }

        public InfractionType Type { get; set; }

        [Required]
        public string Reason { get; set; } = null!;

        public TimeSpan? Duration { get; set; }

        public ulong SubjectId { get; set; }

        public virtual GuildUserEntity Subject { get; set; } = null!;

        public long CreateActionId { get; set; }

        public virtual ModerationActionEntity CreateAction { get; set; } = null!;

        public long? RescindActionId { get; set; }

        public virtual ModerationActionEntity? RescindAction { get; set; }
      
        /// <summary>
        /// A comment about why the infraction was rescinded.
        /// </summary>
        public string? RescindReason { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        public virtual ModerationActionEntity? DeleteAction { get; set; }

        public long? UpdateActionId { get; set; }

        public virtual ModerationActionEntity? UpdateAction { get; set; }

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
                .HasOne(x => x.Subject)
                .WithMany(x => x.Infractions)
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.SubjectId)
                .HasConversion<long>();

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
