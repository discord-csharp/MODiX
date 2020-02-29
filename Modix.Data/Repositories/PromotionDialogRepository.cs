using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    public interface IPromotionDialogRepository
    {
        /// <summary>
        /// Creates a new dialog entity and adds it to the database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task CreateAsync(ulong messageId, long campaignId);

        /// <summary>
        /// gets the current active dialogs
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<PromotionDialogBrief>> GetDialogsAsync();

        /// <summary>
        /// Attempts to delete a poll by its message id
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<bool> TryDeleteAsync(ulong messageId);
    }

    public class PromotionDialogRepository : RepositoryBase, IPromotionDialogRepository
    {
        public PromotionDialogRepository(ModixContext modixContext) : base(modixContext) { }

        public async Task CreateAsync(ulong messageId, long campaignId)
        {
            await ModixContext.Set<PromotionDialogEntity>().AddAsync(new PromotionDialogEntity
            {
                MessageId = messageId,
                CampaignId = campaignId
            });
            await ModixContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<PromotionDialogBrief>> GetDialogsAsync()
            => await ModixContext.Set<PromotionDialogEntity>().AsNoTracking()
                .Select(PromotionDialogBrief.FromEntityExpression)
                .ToListAsync();

        public async Task<bool> TryDeleteAsync(ulong messageId)
        {
            var entity = await ModixContext.Set<PromotionDialogEntity>()
                .FindAsync(messageId);

            if (entity == null )
                return false;

            ModixContext.Set<PromotionDialogEntity>().Remove(entity);
            await ModixContext.SaveChangesAsync();
            return true;
        }
    }
}
