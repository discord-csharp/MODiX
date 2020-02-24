using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    [Table("Infractions")]
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
    }

    public class InfractionEntityConfigurator
        : IEntityTypeConfiguration<InfractionEntity>
    {
        public void Configure(
            EntityTypeBuilder<InfractionEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .HasOne(x => x.Subject)
                .WithMany(x => x.Infractions)
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            entityTypeBuilder
                .Property(x => x.SubjectId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.RescindAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.RescindActionId);

            entityTypeBuilder
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.DeleteActionId);

            entityTypeBuilder
                .HasOne(x => x.UpdateAction)
                .WithOne()
                .HasForeignKey<InfractionEntity>(x => x.UpdateActionId);
        }
    }
}
