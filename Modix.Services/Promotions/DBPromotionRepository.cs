using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotion;

namespace Modix.Services.Promotions
{
    public class DBPromotionRepository : IPromotionRepository
    {
        private readonly ModixContext _context;

        public DBPromotionRepository(ModixContext context)
        {
            _context = context;
        }

        public async Task AddCampaign(PromotionCampaignEntity campaign, SocketGuildUser user)
        {
            var promoUser = await _context.Users.FirstOrDefaultAsync(u => (ulong)u.Id == user.Id);
            if (promoUser == null)
                // TODO: This needs to be done through IUserService. There are concurrency issues if anyone else manages users in the DB directly.
                await _context.Users.AddAsync(new UserEntity
                {
                    Username = $"{user.Username}#{user.Discriminator}",
                    Id = (long)user.Id,
                    Nickname = user.Nickname,
                });

            campaign.PromotionFor = promoUser;

            await _context.PromotionCampaigns.AddAsync(campaign);

            await _context.SaveChangesAsync();
        }

        public async Task AddCommentToCampaign(PromotionCampaignEntity campaign, PromotionCommentEntity comment)
        {
            await campaign.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCampaign(PromotionCampaignEntity campaign)
        {
            _context.Update(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task<PromotionCampaignEntity> GetCampaign(long id)
        {
            return await _context.PromotionCampaigns.FirstOrDefaultAsync(p => p.PromotionCampaignId == id);
        }

        public async Task<IEnumerable<PromotionCampaignEntity>> GetCampaigns()
        {
            return await _context.PromotionCampaigns.ToArrayAsync();
        }
    }
}