using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.Promotions
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<PromotionCampaign>> GetCampaigns();
        Task<PromotionCampaign> GetCampaign(int id);
        Task AddCampaign(PromotionCampaign campaign);
        Task AddCommentToCampaign(PromotionCampaign campaign, PromotionComment comment);
        Task UpdateCampaign(PromotionCampaign campaign);
    }
}
