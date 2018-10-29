using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="PromotionCommentEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IPromotionCommentRepository
    {
        /// <summary>
        /// Begins a new transaction to create comments within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new comment within the repository.
        /// </summary>
        /// <param name="data">The data for the comment to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="PromotionCommentEntity.Id"/> value assigned to the new comment.
        /// </returns>
        Task<long> CreateAsync(PromotionCommentCreationData data);

        /// <summary>
        /// Checks whether the repository contains any comments matching the given search criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="PromotionCommentEntity.Id"/> values to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching comments exist.
        /// </returns>
        Task<bool> AnyAsync(PromotionCommentSearchCriteria searchCriteria);
    }

    /// <inheritdoc />
    public class PromotionCommentRepository : PromotionActionEventRepositoryBase, IPromotionCommentRepository
    {
        /// <summary>
        /// Creates a new <see cref="PromotionCommentRepository"/>, with the injected dependencies
        /// See <see cref="PromotionActionEventRepositoryBase(ModixContext, IEnumerable{IPromotionActionEventHandler})"/> for details.
        /// </summary>
        public PromotionCommentRepository(ModixContext modixContext, IEnumerable<IPromotionActionEventHandler> promotionActionEventHandlers)
            : base(modixContext, promotionActionEventHandlers) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(PromotionCommentCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.PromotionComments.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.CommentId = entity.Id;
            await ModixContext.SaveChangesAsync();

            await RaisePromotionActionCreatedAsync(entity.CreateAction);

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(PromotionCommentSearchCriteria searchCriteria)
            => ModixContext.PromotionComments.AsNoTracking()
                .FilterBy(searchCriteria)
                .AnyAsync();

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
