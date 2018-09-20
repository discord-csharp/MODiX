using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

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

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
