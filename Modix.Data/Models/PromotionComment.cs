using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public enum PromotionSentiment
    {
        For,
        Against,
        Neutral
    }

    public class PromotionComment
    {
        [Key] public long PromotionCommentID { get; set; }

        public DateTimeOffset PostedDate { get; set; }
        public PromotionSentiment Sentiment { get; set; } = PromotionSentiment.Neutral;
        public string Body { get; set; }
        public PromotionCampaign PromotionCampaign { get; set; }

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