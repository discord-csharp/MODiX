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

        public async Task AddCampaign(PromotionCampaign campaign, SocketGuildUser user)
        {
            var promoUser = await _context.Users.FirstOrDefaultAsync(u => (ulong) u.DiscordUserId == user.Id);
            if (promoUser == null)
                await _context.Users.AddAsync(new DiscordUser
                {
                    Username = $"{user.Username}#{user.Discriminator}",
                    DiscordUserId = (long) user.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsBot = false,
                    Nickname = user.Nickname,
                    AvatarUrl = user.GetAvatarUrl()
                });

            campaign.PromotionFor = promoUser;

            await _context.PromotionCampaigns.AddAsync(campaign);

            await _context.SaveChangesAsync();
        }

        public async Task AddCommentToCampaign(PromotionCampaign campaign, PromotionComment comment)
        {
            await campaign.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCampaign(PromotionCampaign campaign)
        {
            _context.Update(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task<PromotionCampaign> GetCampaign(long id)
        {
            return await _context.PromotionCampaigns.FirstOrDefaultAsync(p => p.PromotionCampaignId == id);
        }

        public async Task<IEnumerable<PromotionCampaign>> GetCampaigns()
        {
            return await _context.PromotionCampaigns.ToArrayAsync();
        }
    }
}