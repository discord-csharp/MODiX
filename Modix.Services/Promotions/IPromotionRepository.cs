using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Modix.Data.Models.Promotion;

namespace Modix.Services.Promotions
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<PromotionCampaignEntity>> GetCampaigns();
        Task<PromotionCampaignEntity> GetCampaign(long id);
        Task AddCampaign(PromotionCampaignEntity campaign, SocketGuildUser user);
        Task AddCommentToCampaign(PromotionCampaignEntity campaign, PromotionCommentEntity comment);
        Task UpdateCampaign(PromotionCampaignEntity campaign);
    }
}
