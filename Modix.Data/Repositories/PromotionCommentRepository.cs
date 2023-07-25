using System;
using System.Linq;
using System.Threading.Tasks;
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
        /// containing the action representing the creation.
        /// </returns>
        Task<PromotionActionSummary> CreateAsync(PromotionCommentCreationData data);
        
        /// <summary>
        /// Creates a modified comment within the repository.
        /// </summary>
        /// <param name="commentId">The ID of the comment that is being modified.</param>
        /// <param name="userId">The ID of the user that is modifying the comment.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the action representing the update.
        /// </returns>
        Task<PromotionActionSummary> TryUpdateAsync(long commentId, ulong userId, Action<PromotionCommentMutationData> updateAction);

        /// <summary>
        /// Retrieves information about a promotion comment based on its ID.
        /// </summary>
        /// <param name="commentId">The <see cref="PromotionCommentEntity.Id"/> value of the promotion comment to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested promotion comment, or null if no such comment exists.
        /// </returns>
        Task<PromotionCommentSummary?> ReadSummaryAsync(long commentId);

        /// <summary>
        /// Checks whether the repository contains any comments matching the given search criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="PromotionCommentEntity.Id"/> values to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching comments exist.
        /// </returns>
        Task<bool> AnyAsync(PromotionCommentSearchCriteria searchCriteria);

        Task<PromotionCommentSummary[]> SearchAsync(PromotionCommentSearchCriteria searchCriteria);
    }

    /// <inheritdoc />
    public class PromotionCommentRepository : RepositoryBase, IPromotionCommentRepository
    {
        /// <summary>
        /// Creates a new <see cref="PromotionCommentRepository"/>, with the injected dependencies.
        /// </summary>
        public PromotionCommentRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginUpdateTransactionAsync()
            => _updateTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<PromotionActionSummary> CreateAsync(PromotionCommentCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<PromotionCommentEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.NewCommentId = entity.Id;
            await ModixContext.SaveChangesAsync();

            var action = await ModixContext.Set<PromotionActionEntity>().AsNoTracking()
                .Where(x => x.Id == entity.CreateActionId)
                .AsExpandable()
                .Select(PromotionActionSummary.FromEntityProjection)
                .FirstAsync();

            return action;
        }

        /// <inheritdoc />
        public async Task<PromotionActionSummary> TryUpdateAsync(long commentId, ulong userId, Action<PromotionCommentMutationData> updateAction)
        {
            if (updateAction is null)
                throw new ArgumentNullException(nameof(updateAction));

            var oldComment = await ModixContext.Set<PromotionCommentEntity>()
                                               .Include(x => x.Campaign)
                                               .SingleAsync(x => x.Id == commentId);

            var modifyAction = new PromotionActionEntity
            {
                CampaignId = oldComment.CampaignId,
                Created = DateTimeOffset.UtcNow,
                CreatedById = userId,
                GuildId = oldComment.Campaign.GuildId,
                Type = PromotionActionType.CommentModified,
            };

            await ModixContext.Set<PromotionActionEntity>().AddAsync(modifyAction);
            await ModixContext.SaveChangesAsync();

            var data = PromotionCommentMutationData.FromEntity(oldComment);
            updateAction(data);

            var newComment = data.ToEntity();
            newComment.CreateActionId = modifyAction.Id;

            await ModixContext.Set<PromotionCommentEntity>().AddAsync(newComment);
            await ModixContext.SaveChangesAsync();

            modifyAction.OldCommentId = oldComment.Id;
            modifyAction.NewCommentId = newComment.Id;
            oldComment.ModifyActionId = modifyAction.Id;
            newComment.CreateActionId = modifyAction.Id;
            await ModixContext.SaveChangesAsync();

            var actionSummary = await ModixContext.Set<PromotionActionEntity>().AsNoTracking()
                .Where(x => x.Id == modifyAction.Id)
                .AsExpandable()
                .Select(PromotionActionSummary.FromEntityProjection)
                .FirstAsync();

            return actionSummary;
        }

        /// <inheritdoc />
        public async Task<PromotionCommentSummary?> ReadSummaryAsync(long commentId)
            => await ModixContext.Set<PromotionCommentEntity>().AsNoTracking()
                .Where(x => x.Id == commentId)
                .AsExpandable()
                .Select(PromotionCommentSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public Task<bool> AnyAsync(PromotionCommentSearchCriteria searchCriteria)
            => ModixContext.Set<PromotionCommentEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AnyAsync();

        public async Task<PromotionCommentSummary[]> SearchAsync(PromotionCommentSearchCriteria searchCriteria)
            => await ModixContext.Set<PromotionCommentEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(PromotionCommentSummary.FromEntityProjection)
                .ToArrayAsync();

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _updateTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
