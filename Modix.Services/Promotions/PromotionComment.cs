using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Modix.Services.Promotions
{
    public enum PromotionSentiment
    {
        For,
        Against,
        Neutral
    }

    public class PromotionComment
    {
        public int Id { get; set; }

        public DateTimeOffset PostedDate { get; set; }
        public PromotionSentiment Sentiment { get; set; } = PromotionSentiment.Neutral;
        public string Body { get; set; }

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
                }

                return "?";
            }
        }
    }
}
