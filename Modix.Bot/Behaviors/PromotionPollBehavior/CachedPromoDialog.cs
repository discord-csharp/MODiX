using Discord;
using Modix.Data.Models.Promotions;

namespace Modix.Bot.Behaviors
{
    public class CachedPromoDialog
    {
        public ulong MessageId { get; set; }

        public long CampaignId { get; set; }
    }
}
