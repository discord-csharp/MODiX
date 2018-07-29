using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
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
        public Task<ModerationActionSummary> ReadSummaryAsync(long moderationActionId)
            => ModixContext.ModerationActions.AsNoTracking()
                .Where(x => x.Id == moderationActionId)
                .Select(ModerationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ModerationActionSummary>> SearchSummariesAsync(ModerationActionSearchCriteria searchCriteria)
        {
            return await ModixContext.ModerationActions.AsNoTracking()
                .FilterModerationActionsBy(searchCriteria)
                .Select(ModerationActionSummary.FromEntityProjection)
                .ToArrayAsync();
        }
    }

    internal static class ModerationActionQueryableExtensions
    {
        public static IQueryable<ModerationActionEntity> FilterModerationActionsBy(this IQueryable<ModerationActionEntity> query, ModerationActionSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longCreatedById = (long?)criteria?.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => criteria.Types.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.Created >= criteria.CreatedRange.Value.From,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.Created <= criteria.CreatedRange.Value.To,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreatedById == longCreatedById,
                    longCreatedById != null);
        }
    }
}
