using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Promotions
{
    public class PromotionDialogBrief
    {
        public PromotionDialogBrief(
            ulong messageId,
            long campaignId,
            bool isCampaignOpen)
        {
            MessageId = messageId;
            CampaignId = campaignId;
            IsCampaignOpen = isCampaignOpen;
        }

        public ulong MessageId { get; }
        public long CampaignId { get; }
        public bool IsCampaignOpen { get; }

        public static readonly Expression<Func<PromotionDialogEntity, PromotionDialogBrief>> FromEntityExpression
            = pd => new PromotionDialogBrief(
                pd.MessageId,
                pd.CampaignId,
                pd.Campaign.CloseActionId == null);
    }
}
