using Modix.Data.Models.Promotions;

namespace Modix.Models
{
    public class PromotionCommentData
    {
        public string Body { get; set; }
        public PromotionSentiment Sentiment { get; set; }
    }
}
