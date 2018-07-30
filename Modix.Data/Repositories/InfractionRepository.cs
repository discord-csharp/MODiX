using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Nito.AsyncEx;

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
        public InfractionRepository(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers)
            : base(modixContext)
        {
            ModerationActionEventHandlers = moderationActionEventHandlers ?? throw new ArgumentNullException(nameof(moderationActionEventHandlers));
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

            await ModixContext.Infractions.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.InfractionId = entity.Id;
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreated(entity.CreateActionId, new ModerationActionCreationData()
            {
                GuildId = (ulong)entity.CreateAction.GuildId,
                Type = entity.CreateAction.Type,
                Created = entity.CreateAction.Created,
                CreatedById = (ulong)entity.CreateAction.CreatedById
            });

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<InfractionSummary> ReadSummaryAsync(long infractionId)
            => ModixContext.Infractions.AsNoTracking()
                .Where(x => x.Id == infractionId)
                .Select(InfractionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public Task<bool> AnyAsync(InfractionSearchCriteria criteria)
            => ModixContext.Infractions.AsNoTracking()
                .FilterInfractionsBy(criteria)
                .AnyAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<long>> SearchIdsAsync(InfractionSearchCriteria searchCriteria)
            => await ModixContext.Infractions.AsNoTracking()
                .FilterInfractionsBy(searchCriteria)
                .Select(x => x.Id)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null)
            => await ModixContext.Infractions.AsNoTracking()
                .FilterInfractionsBy(searchCriteria)
                .Select(InfractionSummary.FromEntityProjection)
                .SortBy(sortingCriteria, InfractionSummary.SortablePropertyMap)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var sourceQuery = ModixContext.Infractions.AsNoTracking();

            var filteredQuery = sourceQuery
                .FilterInfractionsBy(searchCriteria);

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
        public async Task<bool> TryRescindAsync(long infractionId, ulong rescindedById)
        {
            var longRescindedById = (long)rescindedById;

            var entity = await ModixContext.Infractions
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.RescindActionId != null))
                return false;

            entity.RescindAction = new ModerationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ModerationActionType.InfractionRescinded,
                Created = DateTimeOffset.Now,
                CreatedById = longRescindedById,
                InfractionId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreated(entity.RescindActionId.Value, new ModerationActionCreationData()
            {
                GuildId = (ulong)entity.RescindAction.GuildId,
                Type = entity.RescindAction.Type,
                Created = entity.RescindAction.Created,
                CreatedById = (ulong)entity.RescindAction.CreatedById
            });

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long infractionId, ulong deletedById)
        {
            var longDeletedById = (long)deletedById;

            var entity = await ModixContext.Infractions
                .Where(x => x.Id == infractionId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            entity.DeleteAction = new ModerationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ModerationActionType.InfractionDeleted,
                Created = DateTimeOffset.Now,
                CreatedById = longDeletedById,
                InfractionId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreated(entity.DeleteActionId.Value, new ModerationActionCreationData()
            {
                GuildId = (ulong)entity.DeleteAction.GuildId,
                Type = entity.DeleteAction.Type,
                Created = entity.DeleteAction.Created,
                CreatedById = (ulong)entity.DeleteAction.CreatedById
            });

            return true;
        }

        /// <summary>
        /// A set of <see cref="IModerationActionEventHandler"/> objects to receive information about moderation actions
        /// affected by this repository.
        /// </summary>
        internal protected IEnumerable<IModerationActionEventHandler> ModerationActionEventHandlers { get; }

        private async Task RaiseModerationActionCreated(long moderationActionId, ModerationActionCreationData data)
        {
            if(ModerationActionEventHandlers.Any())
            {
                foreach(var handler in ModerationActionEventHandlers)
                    await handler.OnModerationActionCreatedAsync(moderationActionId, data);
            }
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }

    internal static class InfractionQueryableExtensions
    {
        public static IQueryable<InfractionEntity> FilterInfractionsBy(this IQueryable<InfractionEntity> query, InfractionSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longSubjectId = (long?)criteria?.SubjectId;
            var longCreatedById = (long?)criteria?.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => criteria.Types.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.SubjectId == longSubjectId,
                    longSubjectId != null)
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange.Value.From,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange.Value.To,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById,
                    longCreatedById != null)
                .FilterBy(
                    x => (x.RescindActionId != null) == criteria.IsRescinded,
                    criteria?.IsRescinded != null)
                .FilterBy(
                    x => (x.DeleteActionId != null) == criteria.IsDeleted,
                    criteria?.IsDeleted != null)
                .FilterBy(
                    x => (x.Duration != null) && (x.CreateAction.Created + x.Duration.Value) >= criteria.CreatedRange.Value.From,
                    criteria?.ExpiresRange?.From != null)
                .FilterBy(
                    x => (x.Duration != null) && (x.CreateAction.Created + x.Duration.Value) >= criteria.CreatedRange.Value.To,
                    criteria?.ExpiresRange?.To != null);
        }
    }
}