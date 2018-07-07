using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Promotion
{
    public enum PromotionSentiment
    {
        For,
        Against,
        Neutral
    }

    public class PromotionCommentEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PromotionCommentId { get; set; }

        public DateTimeOffset PostedDate { get; set; }
        public PromotionSentiment Sentiment { get; set; } = PromotionSentiment.Neutral;
        public string Body { get; set; }
        public PromotionCampaignEntity PromotionCampaign { get; set; }

        [NotMapped]
        public string StatusEmoji
        {
            get
            {
                switch (Sentiment)
                {
                    case PromotionSentiment.For:
                        return "👍";
                    case PromotionSentiment.Against:
                        return "👎";
                    case PromotionSentiment.Neutral:
                        return "😐";
                    default:
                        return "?";
                }
            }
        }
    }
}