using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Reactions;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing reaction entities within an underlying data storage provider.
    /// </summary>
    public interface IReactionRepository
    {
        /// <summary>
        /// Begins a new transaction to maintain reactions within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginMaintainTransactionAsync();

        /// <summary>
        /// Creates a new reaction log within the repository.
        /// </summary>
        /// <param name="data">The data for the reaction to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated identifier value assigned to the new reaction.
        /// </returns>
        Task<long> CreateAsync(ReactionCreationData data);

        /// <summary>
        /// Deletes reaction logs within the repository.
        /// </summary>
        /// <param name="criteria">The criteria for the reactions to be deleted.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete.
        /// </returns>
        Task DeleteAsync(ReactionSearchCriteria criteria);
    }

    /// <inheritdoc />
    public sealed class ReactionRepository : RepositoryBase, IReactionRepository
    {
        /// <summary>
        /// Creates a new <see cref="ReactionRepository"/> with the injected dependencies
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public ReactionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginMaintainTransactionAsync()
            => _maintainTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(ReactionCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Reactions.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(ReactionSearchCriteria criteria)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            var entities = ModixContext.Reactions.FilterBy(criteria);

            ModixContext.RemoveRange(entities);
            await ModixContext.SaveChangesAsync();
        }

        private static readonly RepositoryTransactionFactory _maintainTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
