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
    /// Describes a repository for managing <see cref="DeletedMessageEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IDeletedMessageRepository
    {
        /// <summary>
        /// Begins a new transaction to create deleted messages within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new deleted message within the repository.
        /// </summary>
        /// <param name="data">The data for the deleted message to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="DeletedMessageEntity.Id"/> value assigned to the new deleted message.
        /// </returns>
        Task CreateAsync(DeletedMessageCreationData data);

        /// <summary>
        /// Searches the repository for deleted message information, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="DeletedMessageSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<DeletedMessageSummary>> SearchSummariesPagedAsync(DeletedMessageSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);
    }

    /// <inheritdoc />
    public class DeletedMessageRepository : ModerationActionEventRepositoryBase, IDeletedMessageRepository
    {
        /// <summary>
        /// Creates a new <see cref="DeletedMessageRepository"/>.
        /// See <see cref="ModerationActionEventRepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public DeletedMessageRepository(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers)
            : base(modixContext, moderationActionEventHandlers) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(DeletedMessageCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.DeletedMessages.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.DeletedMessageId = entity.MessageId;
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.CreateAction);
        }

        /// <inheritdoc />
        public async Task<RecordsPage<DeletedMessageSummary>> SearchSummariesPagedAsync(
            DeletedMessageSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            var sourceQuery = ModixContext.DeletedMessages.AsNoTracking();

            var filteredQuery = sourceQuery
                .FilterBy(searchCriteria);

            var pagedQuery = filteredQuery
                .AsExpandable()
                .Select(DeletedMessageSummary.FromEntityProjection)
                .SortBy(sortingCriteria, DeletedMessageSummary.SortablePropertyMap)
                .OrderThenBy(x => x.MessageId, SortDirection.Ascending)
                .PageBy(pagingCriteria);

            return new RecordsPage<DeletedMessageSummary>()
            {
                TotalRecordCount = await sourceQuery.LongCountAsync(),
                FilteredRecordCount = await filteredQuery.LongCountAsync(),
                Records = await pagedQuery.ToArrayAsync(),
            };
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
