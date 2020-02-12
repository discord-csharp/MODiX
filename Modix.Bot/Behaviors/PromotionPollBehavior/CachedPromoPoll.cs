using Discord;
using Modix.Data.Models.Promotions;

namespace Modix.Bot.Behaviors
{
    public class CachedPromoPoll
    {
        public IUserMessage Message { get; set; }

        public PromotionCampaignBrief Campaign { get; set; }

        public int Approve { get; set; }

        public int Disprove { get; set; }

    }
}
