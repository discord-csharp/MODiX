using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="InfractionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IInfractionRepository
    {
        /// <summary>
        /// Begins a new transaction to create infractions within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new infraction within the repository.
        /// </summary>
        /// <param name="data">The data for the infraction to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to the new infraction.
        /// </returns>
        Task<long> CreateAsync(InfractionCreationData data);

        /// <summary>
        /// Retrieves information about an infraction, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested infraction, or null if no such infraction exists.
        /// </returns>
        Task<InfractionSummary?> ReadSummaryAsync(long infractionId);

        /// <summary>
        /// Checks whether the repository contains any infractions matching the given search criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="InfractionEntity.Id"/> values to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching infractions exist.
        /// </returns>
        Task<bool> AnyAsync(InfractionSearchCriteria criteria);

        /// <summary>
        /// Retrieves the <see cref="InfractionSummary.Expires"/> value of the first infraction that matches the given set of criteria (if any).
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting infractions to be checked.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be checked.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested value, or null if no such infractions exist.
        /// </returns>
        Task<DateTimeOffset?> ReadExpiresFirstOrDefaultAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null);

        /// <summary>
        /// Searches the repository for infraction ID values, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionEntity.Id"/> values to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested ID values have been retrieved.</returns>
        Task<IReadOnlyCollection<long>> SearchIdsAsync(InfractionSearchCriteria searchCriteria);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria, and returns the counts of those infractions grouped by type.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IDictionary<InfractionType, int>> GetInfractionCountsAsync(InfractionSearchCriteria searchCriteria);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        /// <summary>
        /// Marks an existing infraction as rescinded, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <param name="rescindedById">The <see cref="UserEntity.Id"/> value of the user that is rescinding the infraction.</param>
        /// <param name="rescindReason">The <see cref="InfractionEntity.RescindReason"/> value of the infraction to be rescinded.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified infraction could be found).
        Task<bool> TryRescindAsync(long infractionId, ulong rescindedById, string? rescindReason = null);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the infraction.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation was successful (I.E. whether the specified infraction could be found).
        Task<bool> TryDeleteAsync(long infractionId, ulong deletedById);

        Task<bool> TryUpdateAsync(long infractionId, string newReason, ulong updatedById);
    }

    /// <inheritdoc />
    public class InfractionRepository : ModerationActionEventRepositoryBase, IInfractionRepository
    {
        /// <summary>
        /// Creates a new <see cref="InfractionRepository"/>.
        /// See <see cref="ModerationActionEventRepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public InfractionRepository(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers, IEnumerable<IInfractionEventHandler> infractionEventHandlers)
            : base(modixContext, moderationActionEventHandlers)
        {
            InfractionEventHandlers = infractionEventHandlers;
        }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(InfractionCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<InfractionEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.InfractionId = entity.Id;
            await ModixContext.SaveChangesAsync();

            await RaiseInfractionCreatedAsync(entity.Id, data);

            await RaiseModerationActionCreatedAsync(entity.CreateAction);

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<InfractionSummary?> ReadSummaryAsync(long infractionId)
            => ModixContext.Set<InfractionEntity>().AsNoTracking()
                .Where(x => x.Id == infractionId)
                .AsExpandable()
                .Select(InfractionSummary.FromEntityProjection)
                .FirstOrDefaultAsync<InfractionSummary?>();

        /// <inheritdoc />
        public Task<bool> AnyAsync(InfractionSearchCriteria criteria)
            => ModixContext.Set<InfractionEntity>().AsNoTracking()
                .FilterBy(criteria)
                .AnyAsync();

        /// <inheritdoc />
        public Task<DateTimeOffset?> ReadExpiresFirstOrDefaultAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null)
            => ModixContext.Set<InfractionEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(InfractionSummary.FromEntityProjection)
                .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
                .Select(x => x.Expires)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<long>> SearchIdsAsync(InfractionSearchCriteria searchCriteria)
            => await ModixContext.Set<InfractionEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .Select(x => x.Id)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria>? sortingCriteria = null)
            => await ModixContext.Set<InfractionEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(InfractionSummary.FromEntityProjection)
                .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IDictionary<InfractionType, int>> GetInfractionCountsAsync(InfractionSearchCriteria searchCriteria)
        {
            // TODO: Group by here is not supported server side right now, should be refactored
            var infractions = await ModixContext.Set<InfractionEntity>()
                .AsNoTracking()
                .FilterBy(searchCriteria)
                .ToArrayAsync();

            var infractionGrouping = infractions
                .GroupBy(x => x.Type);

            //Initialize the returned dictionary so we always have all infraction types present
            var ret = Enum.GetValues(typeof(InfractionType)).Cast<InfractionType>()
                .ToDictionary(x => x, _ => 0);

            foreach (var group in infractionGrouping)
            {
                ret[group.Key] = group.Count();
            }

            return ret;
        }

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var sourceQuery = ModixContext.Set<InfractionEntity>().AsNoTracking().AsExpandable();

            var filteredQuery = sourceQuery
                .FilterBy(searchCriteria);

            var pagedQuery = filteredQuery
                .Select(InfractionSummary.FromEntityProjection)
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
        public async Task<bool> TryRescindAsync(long infractionId, ulong rescindedById, string? rescindReason = null)
        {
            var entity = await ModixContext.Set<InfractionEntity>()
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.RescindActionId != null))
                return false;

            entity.RescindAction = new ModerationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ModerationActionType.InfractionRescinded,
                Created = DateTimeOffset.UtcNow,
                CreatedById = rescindedById,
                InfractionId = entity.Id
            };
            entity.RescindReason = rescindReason;

            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.RescindAction);

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long infractionId, ulong deletedById)
        {
            var entity = await ModixContext.Set<InfractionEntity>()
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            entity.DeleteAction = new ModerationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ModerationActionType.InfractionDeleted,
                Created = DateTimeOffset.UtcNow,
                CreatedById = deletedById,
                InfractionId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.DeleteAction);

            return true;
        }

        public async Task<bool> TryUpdateAsync(long infractionId, string newReason, ulong updatedById)
        {
            var entity = await ModixContext.Set<InfractionEntity>()
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var originalReason = entity.Reason;

            entity.Reason = newReason;

            entity.UpdateAction = new ModerationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ModerationActionType.InfractionUpdated,
                Created = DateTimeOffset.UtcNow,
                CreatedById = updatedById,
                InfractionId = entity.Id,
                OriginalInfractionReason = originalReason,
            };

            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.UpdateAction);

            return true;
        }

        /// <summary>
        /// A set of <see cref="IInfractionEventHandler"/> objects to receive information about infractions.
        /// affected by this repository.
        /// </summary>
        internal protected IEnumerable<IInfractionEventHandler> InfractionEventHandlers { get; }

        private async Task RaiseInfractionCreatedAsync(long infractionId, InfractionCreationData data)
        {
            foreach (var handler in InfractionEventHandlers)
                await handler.OnInfractionCreatedAsync(infractionId, data);
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
