using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Modix.Services.Promotions
{
    public enum CampaignStatus
    {
        Active,
        Approved,
        Denied
    }

    public class PromotionCampaign
    {
        public int Id { get; set; }

        public ulong UserId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public CampaignStatus Status { get; set; }

        [JsonIgnore]
        public int ForVotes => Comments.Where(d => d.Sentiment == PromotionSentiment.For).Count();
        [JsonIgnore]
        public int AgainstVotes => Comments.Where(d => d.Sentiment == PromotionSentiment.Against).Count();
        [JsonIgnore]
        public int TotalVotes => Comments.Where(d => d.Sentiment != PromotionSentiment.Neutral).Count();

        [JsonIgnore]
        public float VoteRatio => ((float)ForVotes / (ForVotes + AgainstVotes));
        
        [JsonIgnore]
        public string SentimentIcon
        {
            get
            {
                if (VoteRatio > 0.67)
                {
                    return "👍";
                }

                if (VoteRatio > 0.34)
                {
                    return "😐";
                }

                return "👎";
            }
        }

        public List<PromotionComment> Comments { get; set; } = new List<PromotionComment>();
    }
}
