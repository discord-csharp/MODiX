using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
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

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .Property(x => x.SubjectId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .Property(x => x.TargetRoleId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasOne(x => x.Subject)
                .WithMany(x => x.PromotionCampaigns)
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .Property(x => x.Outcome)
                .HasConversion<string>();

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<PromotionCampaignEntity>(x => x.CreateActionId);

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasOne(x => x.CloseAction)
                .WithOne()
                .HasForeignKey<PromotionCampaignEntity>(x => x.CloseActionId);

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasIndex(x => x.GuildId);

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasIndex(x => x.SubjectId);

            modelBuilder
                .Entity<PromotionCampaignEntity>()
                .HasIndex(x => x.Outcome);
        }
    }
}
