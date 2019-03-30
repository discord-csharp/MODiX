using MediatR;
using Modix.Data.Models.Promotions;

namespace Modix.Services.Messages.Modix
{
    public class PromotionActionCreated : INotification
    {
        public long PromotionActionId { get; set; }

        public PromotionActionCreationData PromotionActionCreationData { get; set; }
    }
}
