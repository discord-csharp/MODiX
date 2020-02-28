using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    [Table("PromotionDialogs")]
    public class PromotionDialogEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        public long CampaignId { get; set; }

        /// <summary>
        /// The <see cref="PromotionCampaignEntity"/> to which this comment belongs.
        /// </summary>
        [ForeignKey(nameof(CampaignId))]
        public virtual PromotionCampaignEntity Campaign { get; set; } = null!;
    }

    public class PromotionDialogConfiguration : IEntityTypeConfiguration<PromotionDialogEntity>
    {
        public void Configure(EntityTypeBuilder<PromotionDialogEntity> builder)
        {
            builder
                .Property(x => x.MessageId)
                .HasConversion<long>();

            builder
                .Property(x => x.CampaignId);

            builder
                .HasOne(x => x.Campaign)
                .WithOne()
                .HasForeignKey<PromotionDialogEntity>(x => x.CampaignId);
        }
    }
}
