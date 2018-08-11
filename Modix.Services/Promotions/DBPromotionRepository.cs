using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotion;
using Modix.Services.Core;

namespace Modix.Services.Promotions
{
    public class DBPromotionRepository : IPromotionRepository
    {
        private readonly ModixContext _context;
        private readonly IUserService _userService;

        public DBPromotionRepository(ModixContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<PromotionCampaignEntity> AddCampaign(PromotionCampaignEntity campaign, SocketGuildUser user)
        {
            await _userService.TrackUserAsync(user);

            campaign.PromotionFor = _context.Users.Find((long)user.Id);
            var result = await _context.PromotionCampaigns.AddAsync(campaign);

            await _context.SaveChangesAsync();

            return result.Entity;
        }

        public async Task AddCommentToCampaign(PromotionCampaignEntity campaign, PromotionCommentEntity comment)
        {
            comment.PromotionCampaign = campaign;
            await _context.PromotionComments.AddAsync(comment);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCampaign(PromotionCampaignEntity campaign)
        {
            _context.Update(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task<PromotionCampaignEntity> GetCampaign(long id)
        {
            return await _context.PromotionCampaigns
                .Include(d=>d.PromotionFor)
                .Include(d=>d.Comments)
                .FirstOrDefaultAsync(p => p.PromotionCampaignId == id);
        }

        public async Task<IEnumerable<PromotionCampaignEntity>> GetCampaigns()
        {
            var result = await _context.PromotionCampaigns
                .Include(d => d.PromotionFor)
                .Include(d => d.Comments)
                .ToArrayAsync();

            return result ?? new PromotionCampaignEntity[0];
        }
    }
}