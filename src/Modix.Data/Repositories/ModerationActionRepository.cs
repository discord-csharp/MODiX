using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ModerationActionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IModerationActionRepository
    {
        /// <summary>
        /// Retrieves information about a moderation action from the repository.
        /// </summary>
        /// <param name="moderationActionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested moderation action, or null if no such action exists.
        /// </returns>
        Task<ModerationActionSummary?> ReadSummaryAsync(long moderationActionId);

        /// <summary>
        /// Searches the repository for moderation action information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">A set of criteria defining the moderation actions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested moderation actions.
        /// </returns>
        Task<IReadOnlyCollection<ModerationActionSummary>> SearchSummariesAsync(ModerationActionSearchCriteria searchCriteria);
    }

    /// <inheritdoc />
    public class ModerationActionRepository : RepositoryBase, IModerationActionRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ModerationActionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<ModerationActionSummary?> ReadSummaryAsync(long moderationActionId)
            => ModixContext.Set<ModerationActionEntity>().AsNoTracking()
                .Where(x => x.Id == moderationActionId)
                .AsExpandable()
                .Select(ModerationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ModerationActionSummary>> SearchSummariesAsync(ModerationActionSearchCriteria searchCriteria)
        {
            return await ModixContext.Set<ModerationActionEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(ModerationActionSummary.FromEntityProjection)
                .ToArrayAsync();
        }
    }
}
