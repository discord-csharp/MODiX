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
        public async Task<long> CreateAsync(InfractionCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();
            entity.Created = DateTimeOffset.Now;

            await ModixContext.Infractions.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<InfractionSummary> ReadAsync(long infractionId)
            => ModixContext.Infractions.AsNoTracking()
                .Where(x => x.Id == infractionId)
                .Select(InfractionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria)
            => await SearchInfractionsBy(ModixContext.Infractions.AsNoTracking(), searchCriteria)
                .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var sourceQuery = ModixContext.Infractions.AsNoTracking();

            var filteredQuery = SearchInfractionsBy(sourceQuery, searchCriteria);

            var pagedQuery = filteredQuery
                .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
                // Always sort by Id last, otherwise ordering of records with matching fields is not guaranteed by the DB
                .OrderThenBy(x => x.Id, SortDirection.Ascending)
                .PageBy(pagingCriteria);

            return new RecordsPage<InfractionSummary>()
            {
                TotalRecordCount = await sourceQuery.LongCountAsync(),
                FilteredRecordCount = await filteredQuery.LongCountAsync(),
                Records = await pagedQuery.ToArrayAsync()
            };
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(long infractionId, Action<InfractionMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.Infractions
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = InfractionMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.RescindActionId);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        private static IQueryable<InfractionSummary> SearchInfractionsBy(IQueryable<InfractionEntity> query, InfractionSearchCriteria criteria)
            =>  (criteria == null)
                ? query
                    .Select(InfractionSummary.FromEntityProjection)
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
                    .Select(InfractionSummary.FromEntityProjection)
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