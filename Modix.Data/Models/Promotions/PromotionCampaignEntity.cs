using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a campaign to determine if a user should be awarded a promotion in rank.
    /// </summary>
    public class PromotionCampaignEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="PromotionCampaignEntity"/>.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this promotion action applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="Subject"/>.
        /// </summary>
        [Required]
        public ulong SubjectId { get; set; }

        /// <summary>
        /// The user whose promotion is being proposed by this campaign.
        /// </summary>
        [Required]
        public virtual GuildUserEntity Subject { get; set; }

        /// <summary>
        /// The <see cref="GuildRoleEntity.RoleId"/> value of <see cref="TargetRole"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(TargetRole))]
        public ulong TargetRoleId { get; set; }

        /// <summary>
        /// The role to which <see cref="Subject"/> should be promoted.
        /// </summary>
        [Required]
        public virtual GuildRoleEntity TargetRole { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity"/> that created this <see cref="PromotionCampaignEntity"/>.
        /// </summary>
        [Required]
        public virtual PromotionActionEntity CreateAction { get; set; }

        /// <summary>
        /// The outcome of the campaign, I.E. the result of processing <see cref="CloseAction"/>.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity.Id"/> value of <see cref="CloseAction"/>.
        /// </summary>
        public long? CloseActionId { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity"/> that closed this <see cref="PromotionCampaignEntity"/>.
        /// </summary>
        public virtual PromotionActionEntity CloseAction { get; set; }

        /// <summary>
        /// The set of comments that have been recorded for this campaign.
        /// </summary>
        public virtual ICollection<PromotionCommentEntity> Comments { get; set; }

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
                .WithMany()
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
        }
    }
}