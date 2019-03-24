using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Emoji;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing emoji entities within an underlying data storage provider.
    /// </summary>
    public interface IEmojiRepository
    {
        /// <summary>
        /// Begins a new transaction to maintain emoji within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginMaintainTransactionAsync();

        /// <summary>
        /// Creates a new emoji log within the repository.
        /// </summary>
        /// <param name="data">The data for the emoji log to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated identifier value assigned to the new emoji log.
        /// </returns>
        Task<long> CreateAsync(EmojiCreationData data);

        /// <summary>
        /// Creates multiple new emoji logs within the repository.
        /// </summary>
        /// <param name="data">The data for the emoji logs to be created.</param>
        /// <param name="count">The number of new logs to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task CreateMultipleAsync(EmojiCreationData data, int count);

        /// <summary>
        /// Deletes emoji logs within the repository.
        /// </summary>
        /// <param name="criteria">The criteria for the emoji to be deleted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task DeleteAsync(EmojiSearchCriteria criteria);

        /// <summary>
        /// Returns the number of times emoji matching the specified criteria occurred.
        /// </summary>
        /// <param name="criteria">The criteria for the emoji to be counted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a dictionary of emoji and their counts.
        /// </returns>
        Task<IReadOnlyDictionary<EphemeralEmoji, int>> GetCountsAsync(EmojiSearchCriteria criteria);

        /// <summary>
        /// Searches the emoji logs for emoji records matching the supplied criteria.
        /// </summary>
        /// <param name="criteria">The criteria with which to filter the emoji returned.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="criteria"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a collection of emoji meeting the supplied criteria.
        /// </returns>
        Task<IReadOnlyCollection<EmojiSummary>> SearchSummariesAsync(EmojiSearchCriteria criteria);
    }

    /// <inheritdoc />
    public sealed class EmojiRepository : RepositoryBase, IEmojiRepository
    {
        /// <summary>
        /// Creates a new <see cref="EmojiRepository"/> with the injected dependencies
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public EmojiRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginMaintainTransactionAsync()
            => _maintainTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(EmojiCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Emoji.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task CreateMultipleAsync(EmojiCreationData data, int count)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (count <= 0)
                return;

            var now = DateTimeOffset.Now;
            var entities = Enumerable.Range(0, count).Select(_ => data.ToEntity(now));

            await ModixContext.Emoji.AddRangeAsync(entities);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var entities = ModixContext.Emoji.FilterBy(criteria);

            ModixContext.RemoveRange(entities);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<EphemeralEmoji, int>> GetCountsAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var emoji = await ModixContext.Emoji.AsNoTracking()
                .FilterBy(criteria)
                .GroupBy(x => new { x.EmojiId, x.EmojiName })
                .ToArrayAsync();

            var counts = emoji.ToDictionary(
                x => EphemeralEmoji.FromRawData(x.Key.EmojiName, x.Key.EmojiId),
                x => x.Count(),
                new EphemeralEmoji.EqualityComparer());

            return counts;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<EmojiSummary>> SearchSummariesAsync(EmojiSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var emoji = await ModixContext.Emoji.AsNoTracking()
                .FilterBy(criteria)
                .AsExpandable()
                .Select(EmojiSummary.FromEntityProjection)
                .ToArrayAsync();

            return emoji;
        }

        private static readonly RepositoryTransactionFactory _maintainTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
