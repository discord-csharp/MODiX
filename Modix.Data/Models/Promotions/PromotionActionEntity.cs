using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an action performed within the promotions system.
    /// </summary>
    [Table("PromotionActions")]
    public class PromotionActionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="PromotionActionEntity"/>.
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
        /// A timestamp indicating when this action was performed.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The type of action that was performed.
        /// </summary>
        [Required]
        public PromotionActionType Type { get; set; }

        /// <summary>
        /// The <see cref="PromotionActionEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required]
        public ulong CreatedById { get; set; }

        /// <summary>
        /// The staff member that performed this action.
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; } = null!;

        /// <summary>
        /// The <see cref="PromotionCampaignEntity.Id"/> value of <see cref="Campaign"/>, if any.
        /// </summary>
        [ForeignKey(nameof(Campaign))]
        public long? CampaignId { get; set; }

        /// <summary>
        /// The <see cref="PromotionCampaignEntity"/> to which this <see cref="PromotionActionEntity"/> applies.
        /// Null, if an <see cref="PromotionCampaignEntity"/> was involved in this <see cref="PromotionActionEntity"/>.
        /// </summary>
        public virtual PromotionCampaignEntity? Campaign { get; set; }

        /// <summary>
        /// The <see cref="PromotionCommentEntity.Id"/> value of <see cref="NewComment"/>, if any.
        /// </summary>
        [ForeignKey(nameof(NewComment))]
        public long? NewCommentId { get; set; }

        /// <summary>
        /// The <see cref="PromotionCommentEntity"/> to which this <see cref="PromotionActionEntity"/> applies.
        /// Null, if a <see cref="PromotionCommentEntity"/> was not involved in this <see cref="PromotionActionEntity"/>.
        /// </summary>
        public virtual PromotionCommentEntity? NewComment { get; set; }

        /// <summary>
        /// The <see cref="PromotionCommentEntity.Id"/> value of <see cref="OldComment"/>, if any.
        /// </summary>
        [ForeignKey(nameof(OldComment))]
        public long? OldCommentId { get; set; }

        /// <summary>
        /// The old <see cref="PromotionCommentEntity"/> to which this <see cref="PromotionActionEntity"/> applies.
        /// Null, if a <see cref="PromotionCommentEntity"/> was not involved in this <see cref="PromotionActionEntity"/>.
        /// </summary>
        public virtual PromotionCommentEntity? OldComment { get; set; }
    }

    public class PromotionActionEntityConfigurator
        : IEntityTypeConfiguration<PromotionActionEntity>
    {
        public void Configure(
            EntityTypeBuilder<PromotionActionEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .Property(x => x.CreatedById)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });

            entityTypeBuilder
                .HasOne(x => x.OldComment)
                .WithOne()
                .HasForeignKey<PromotionActionEntity>(x => x.OldCommentId);

            entityTypeBuilder
                .HasOne(x => x.NewComment)
                .WithOne()
                .HasForeignKey<PromotionActionEntity>(x => x.NewCommentId);
        }
    }
}
