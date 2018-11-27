using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="PromotionCommentMessageEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IPromotionCommentMessageRepository
    {
        /// <summary>
        /// Creates a new comment message within the repository.
        /// </summary>
        /// <param name="data">The data for the comment message to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task CreateAsync(PromotionCommentMessageCreationData data);

        /// <summary>
        /// Retrieves all promotion comment messages with the supplied <paramref name="commentId"/>.
        /// </summary>
        /// <param name="commentId">The <see cref="PromotionCommentMessageEntity.CommentId"/> value of the promotion comment messages to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested promotion comment messages, or null if no such comment messages exist.
        /// </returns>
        Task<IReadOnlyCollection<PromotionCommentMessageSummary>> ReadSummariesAsync(long commentId);
    }

    /// <inheritdoc />
    public class PromotionCommentMessageRepository : RepositoryBase, IPromotionCommentMessageRepository
    {
        /// <summary>
        /// Creates a new <see cref="PromotionCommentMessageRepository"/> with the injected dependencies.
        /// </summary>
        public PromotionCommentMessageRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task CreateAsync(PromotionCommentMessageCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.PromotionCommentMessages.AddAsync(entity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<PromotionCommentMessageSummary>> ReadSummariesAsync(long commentId)
            => await ModixContext.PromotionCommentMessages.AsNoTracking()
                .Where(x => x.CommentId == commentId)
                .Include(x => x.Comment)
                .Include(x => x.Channel)
                .AsExpandable()
                .Select(PromotionCommentMessageSummary.FromEntityProjection)
                .ToArrayAsync();
    }
}
