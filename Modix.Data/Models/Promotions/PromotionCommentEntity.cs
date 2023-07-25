using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a comment made in reference to a <see cref="PromotionCampaignEntity"/>,
    /// regarding whether or not the proposed promotion should be accepted or rejected.
    /// </summary>
    [Table("PromotionComments")]
    public class PromotionCommentEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="PromotionCommentEntity"/>.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The <see cref="PromotionCampaignEntity.Id"/> value of <see cref="Campaign"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Campaign))]
        public long CampaignId { get; set; }

        /// <summary>
        /// The <see cref="PromotionCampaignEntity"/> to which this comment belongs.
        /// </summary>
        [Required]
        public virtual PromotionCampaignEntity Campaign { get; set; } = null!;

        /// <summary>
        /// The commenter's sentiment, regarding the outcome of <see cref="Campaign"/>.
        /// </summary>
        [Required]
        public PromotionSentiment Sentiment { get; set; }

        /// <summary>
        /// The text content of the comment, supplied by the commenter.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity"/> that created this comment.
        /// </summary>
        [Required]
        public virtual PromotionActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The <see cref="PromotionActionEntity.Id"/> value of <see cref="ModifyAction"/>.
        /// </summary>
        public long? ModifyActionId { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity"/> that modified this comment.
        /// </summary>
        public virtual PromotionActionEntity? ModifyAction { get; set; }
    }

    public class PromotionCommentEntityConfigurator
        : IEntityTypeConfiguration<PromotionCommentEntity>
    {
        public void Configure(
            EntityTypeBuilder<PromotionCommentEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Sentiment)
                .HasConversion<string>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<PromotionCommentEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.ModifyAction)
                .WithOne()
                .HasForeignKey<PromotionCommentEntity>(x => x.ModifyActionId);
        }
    }
}
