using MediatR;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Messages
{
    public class PromotionActionCreated : INotification
    {
        public long PromotionActionId { get; set; }
        public PromotionActionCreationData PromotionActionCreationData { get; set; }
    }
}
