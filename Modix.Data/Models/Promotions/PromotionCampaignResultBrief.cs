using System;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    public class PromotionCampaignResultBrief
    {
        public ulong GuildId { get; set; }

        public ulong SubjectId { get; set; }

        public ulong TargetRoleId { get; set; }

        public DateTimeOffset Closed { get; set; }

        public PromotionCampaignOutcome Outcome { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Query<PromotionCampaignResultBrief>()
                .Property(x => x.Outcome)
                .HasConversion<string>();
        }
    }
}
