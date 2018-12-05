using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modix.Data.ExpandableQueries;
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
        /// Begins a new transaction to update comments within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginUpdateTransactionAsync();

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
        /// Creates a modified comment within the repository.
        /// </summary>
        /// <param name="commentId">The ID of the comment that is being modified.</param>
        /// <param name="userId">The ID of the user that is modifying the comment.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="PromotionCommentEntity.Id"/> value assigned to the new comment.
        /// </returns>
        Task<long> TryUpdateAsync(long commentId, ulong userId, Action<PromotionCommentMutationData> updateAction);

        /// <summary>
        /// Retrieves information about a promotion comment based on its ID.
        /// </summary>
        /// <param name="commentId">The <see cref="PromotionCommentEntity.Id"/> value of the promotion comment to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested promotion comment, or null if no such comment exists.
        /// </returns>
        Task<PromotionCommentSummary> ReadSummaryAsync(long commentId);

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
        /// See <see cref="PromotionActionEventRepositoryBase"/> for details.
        /// </summary>
        public PromotionCommentRepository(ModixContext modixContext, IMediator mediator)
            : base(modixContext, mediator) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginUpdateTransactionAsync()
            => _updateTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(PromotionCommentCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.PromotionComments.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.NewCommentId = entity.Id;
            await ModixContext.SaveChangesAsync();

            await RaisePromotionActionCreatedAsync(entity.CreateAction);

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task<long> TryUpdateAsync(long commentId, ulong userId, Action<PromotionCommentMutationData> updateAction)
        {
            if (updateAction is null)
                throw new ArgumentNullException(nameof(updateAction));

            var oldComment = await ModixContext.PromotionComments
                                               .Include(x => x.Campaign)
                                               .SingleAsync(x => x.Id == commentId);

            var modifyAction = new PromotionActionEntity
            {
                CampaignId = oldComment.CampaignId,
                Created = DateTimeOffset.Now,
                CreatedById = userId,
                GuildId = oldComment.Campaign.GuildId,
                Type = PromotionActionType.CommentModified,
            };

            await ModixContext.PromotionActions.AddAsync(modifyAction);
            await ModixContext.SaveChangesAsync();

            var data = PromotionCommentMutationData.FromEntity(oldComment);
            updateAction(data);

            var newComment = data.ToEntity();
            newComment.CreateActionId = modifyAction.Id;

            await ModixContext.PromotionComments.AddAsync(newComment);
            await ModixContext.SaveChangesAsync();

            modifyAction.OldCommentId = oldComment.Id;
            modifyAction.NewCommentId = newComment.Id;
            oldComment.ModifyActionId = modifyAction.Id;
            newComment.CreateActionId = modifyAction.Id;
            await ModixContext.SaveChangesAsync();

            await RaisePromotionActionCreatedAsync(modifyAction);

            return newComment.Id;
        }

        /// <inheritdoc />
        public async Task<PromotionCommentSummary> ReadSummaryAsync(long commentId)
            => await ModixContext.PromotionComments.AsNoTracking()
                .Where(x => x.Id == commentId)
                .AsExpandable()
                .Select(PromotionCommentSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public Task<bool> AnyAsync(PromotionCommentSearchCriteria searchCriteria)
            => ModixContext.PromotionComments.AsNoTracking()
                .FilterBy(searchCriteria)
                .AnyAsync();

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _updateTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
