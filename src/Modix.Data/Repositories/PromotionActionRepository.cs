using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="PromotionActionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IPromotionActionRepository
    {
        /// <summary>
        /// Retrieves information about a promotion action from the repository.
        /// </summary>
        /// <param name="promotionActionId">The <see cref="PromotionActionEntity.Id"/> value of the promotion action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested promotion action, or null if no such action exists.
        /// </returns>
        Task<PromotionActionSummary?> ReadSummaryAsync(long promotionActionId);
    }

    /// <inheritdoc />
    public class PromotionActionRepository : RepositoryBase, IPromotionActionRepository
    {
        public PromotionActionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<PromotionActionSummary?> ReadSummaryAsync(long promotionActionId)
            => ModixContext.Set<PromotionActionEntity>().AsNoTracking()
                .Where(x => x.Id == promotionActionId)
                .AsExpandable()
                .Select(PromotionActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
    }
}
