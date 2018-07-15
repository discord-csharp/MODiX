using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotion
{
    public enum CampaignStatus
    {
        Active,
        Approved,
        Denied
    }

    public class PromotionCampaignEntity
    {
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PromotionCampaignId { get; set; }

        [Required]
        public UserEntity PromotionFor { get; set; }

        public DateTimeOffset StartDate { get; set; }
        public CampaignStatus Status { get; set; }

        [NotMapped]
        public int ForVotes => Comments.Count(d => d.Sentiment == PromotionSentiment.For);

        [NotMapped]
        public int AgainstVotes => Comments.Count(d => d.Sentiment == PromotionSentiment.Against);

        [NotMapped]
        public int TotalVotes => Comments.Count(d => d.Sentiment != PromotionSentiment.Neutral);

        [NotMapped]
        public float VoteRatio => (float) ForVotes / (ForVotes + AgainstVotes);

        [NotMapped]
        public string SentimentIcon
        {
            get
            {
                if (VoteRatio > 0.67) return "👍";

                return VoteRatio > 0.34 ? "😐" : "👎";
            }
        }

        [InverseProperty("PromotionCampaign")]
        public DbSet<PromotionCommentEntity> Comments { get; set; }
    }
}