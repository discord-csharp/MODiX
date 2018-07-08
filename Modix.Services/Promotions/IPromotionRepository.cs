using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Modix.Data.Models;

namespace Modix.Services.Promotions
{
    public interface IPromotionRepository
    {
        Task<IEnumerable<PromotionCampaign>> GetCampaigns();
        Task<PromotionCampaign> GetCampaign(long id);
        Task AddCampaign(PromotionCampaign campaign, SocketGuildUser user);
        Task AddCommentToCampaign(PromotionCampaign campaign, PromotionComment comment);
        Task UpdateCampaign(PromotionCampaign campaign);
    }
}
