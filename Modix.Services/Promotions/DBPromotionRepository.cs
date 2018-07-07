using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models;

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
            var promoUser = await _context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordUserId == user.Id);
            if (promoUser == null)
                await _context.DiscordUsers.AddAsync(new DiscordUserEntity
                {
                    Username = $"{user.Username}#{user.Discriminator}",
                    DiscordUserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsBot = false,
                    Nickname = user.Nickname,
                    AvatarUrl = user.GetAvatarUrl()
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