using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    public class PromotionSentimentCount
    {
        public PromotionSentiment Sentiment { get; set; }

        public int Count { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Query<PromotionSentimentCount>()
                .Property(x => x.Sentiment)
                .HasConversion<string>();
        }
    }
}
