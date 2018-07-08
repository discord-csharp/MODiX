using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class InfractionRepository : RepositoryBase, IInfractionRepository
    {
        /// <summary>
        /// Creates a new <see cref="InfractionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public InfractionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task<long> InsertAsync(InfractionEntity infraction)
        {
            if (infraction == null)
                throw new ArgumentNullException(nameof(infraction));

            await ModixContext.Infractions.AddAsync(infraction);

            await ModixContext.SaveChangesAsync();

            return infraction.Id;
        }

        /// <inheritdoc />
        public async Task SetRescindActionAsync(long infractionId, long rescindActionId)
        {
            var infraction = await ModixContext.Infractions
                .SingleAsync(x => x.Id == infractionId);

            infraction.RescindActionId = rescindActionId;

            ModixContext.UpdateProperty(infraction, x => x.RescindActionId);

            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSearchResult>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria)
            => await SearchInfractionsBy(ModixContext.Infractions.AsNoTracking(), searchCriteria)
                .SortBy(sortingCriteria, InfractionSearchResult.SortablePropertyMap)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSearchResult>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var sourceQuery = ModixContext.Infractions.AsNoTracking();

            var filteredQuery = SearchInfractionsBy(sourceQuery, searchCriteria);

            var pagedQuery = filteredQuery
                .SortBy(sortingCriteria, InfractionSearchResult.SortablePropertyMap)
                // Always sort by Id last, otherwise ordering of records with matching fields is not guaranteed by the DB
                .OrderThenBy(x => x.Id, SortDirection.Ascending)
                .PageBy(pagingCriteria);

            return new RecordsPage<InfractionSearchResult>()
            {
                TotalRecordCount = await sourceQuery.LongCountAsync(),
                FilteredRecordCount = await filteredQuery.LongCountAsync(),
                Records = await pagedQuery.ToArrayAsync()
            };
        }

        private static IQueryable<InfractionSearchResult> SearchInfractionsBy(IQueryable<InfractionEntity> query, InfractionSearchCriteria criteria)
            =>  (criteria == null)
                ? query
                    .Select(InfractionSearchResult.FromEntityProjection)
                : query
                    .FilterBy(
                        x => criteria.Types.Contains(x.Type),
                        (criteria.Types != null) && criteria.Types.Any())
                    .FilterBy(
                        x => x.SubjectId == criteria.SubjectId.Value,
                        (criteria.Types != null) && criteria.Types.Any())
                    .FilterBy(
                        x => x.CreateAction.CreatedById == criteria.CreatedById.Value,
                        criteria.CreatedById.HasValue)
                    .FilterBy(
                        x => x.RescindActionId.HasValue == criteria.IsRescinded.Value,
                        criteria.IsRescinded.HasValue)
                    .Select(InfractionSearchResult.FromEntityProjection)
                    .FilterBy(
                        x => x.CreateAction.Created >= criteria.CreatedRange.Value.From.Value,
                        (criteria.CreatedRange.HasValue) && (criteria.CreatedRange.Value.From.HasValue))
                    .FilterBy(
                        x => x.CreateAction.Created <= criteria.CreatedRange.Value.To.Value,
                        (criteria.CreatedRange.HasValue) && (criteria.CreatedRange.Value.To.HasValue))
                    .FilterBy(
                        x => x.IsExpired == criteria.IsExpired.Value,
                        criteria.IsExpired.HasValue);
    }
}