using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    [Table("PromotionDialogs")]
    public class PromotionDialogEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong MessageId { get; set; }

        public long CampaignId { get; set; }
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
        }
    }
}
