using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    [Table("PromotionCampaigns")]
    public class PromotionCampaignEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ulong GuildId { get; set; }

        public ulong SubjectId { get; set; }

        public virtual GuildUserEntity Subject { get; set; } = null!;

        [ForeignKey(nameof(TargetRole))]
        public ulong TargetRoleId { get; set; }

        public virtual GuildRoleEntity TargetRole { get; set; } = null!;

        public long CreateActionId { get; set; }

        public virtual PromotionActionEntity CreateAction { get; set; } = null!;

        public PromotionCampaignOutcome? Outcome { get; set; }

        public long? CloseActionId { get; set; }

        public virtual PromotionActionEntity? CloseAction { get; set; }

        public virtual ICollection<PromotionCommentEntity> Comments { get; set; } = null!;
    }

    public class PromotionCampaignEntityConfigurator
        : IEntityTypeConfiguration<PromotionCampaignEntity>
    {
        public void Configure(
            EntityTypeBuilder<PromotionCampaignEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.SubjectId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.TargetRoleId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.Subject)
                .WithMany(x => x.PromotionCampaigns)
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            entityTypeBuilder
                .Property(x => x.Outcome)
                .HasConversion<string>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<PromotionCampaignEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.CloseAction)
                .WithOne()
                .HasForeignKey<PromotionCampaignEntity>(x => x.CloseActionId);

            entityTypeBuilder
                .HasIndex(x => x.GuildId);

            entityTypeBuilder
                .HasIndex(x => x.SubjectId);

            entityTypeBuilder
                .HasIndex(x => x.Outcome);
        }
    }
}
