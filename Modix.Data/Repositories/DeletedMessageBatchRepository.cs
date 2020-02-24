using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="DeletedMessageBatchEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IDeletedMessageBatchRepository
    {
        /// <summary>
        /// Begins a new transaction to create deleted message batches within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a sequence of new deleted messages within the repository.
        /// </summary>
        /// <param name="data">The data for the deleted messages to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> that will complete when the operation completes.</returns>
        Task CreateAsync(DeletedMessageBatchCreationData data);
    }

    /// <inheritdoc />
    public class DeletedMessageBatchRepository : ModerationActionEventRepositoryBase, IDeletedMessageBatchRepository
    {
        /// <summary>
        /// Creates a new <see cref="DeletedMessageBatchRepository"/>.
        /// See <see cref="ModerationActionEventRepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public DeletedMessageBatchRepository(ModixContext modixContext, IEnumerable<IModerationActionEventHandler> moderationActionEventHandlers)
            : base(modixContext, moderationActionEventHandlers) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(DeletedMessageBatchCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<DeletedMessageBatchEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.DeletedMessageBatchId = entity.Id;
            await ModixContext.SaveChangesAsync();

            var deletedMessageEntities = data.Data.Select(x =>
            {
                x.BatchId = entity.Id;
                return x.ToBatchEntity();
            });

            await ModixContext.Set<DeletedMessageEntity>().AddRangeAsync(deletedMessageEntities);
            await ModixContext.SaveChangesAsync();

            await RaiseModerationActionCreatedAsync(entity.CreateAction);
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
