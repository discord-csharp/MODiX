using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Mentions;

namespace Modix.Data.Repositories
{
    public interface IMentionMappingRepository
    {
        /// <summary>
        /// Begins a new transaction to create mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other create transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Begins a new transaction to update mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other create transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginUpdateTransactionAsync();

        /// <summary>
        /// Begins a new transaction to delete mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other delete transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginDeleteTransactionAsync();

        /// <summary>
        /// Creates a new mention mapping within the repository.
        /// </summary>
        /// <param name="data">The data for the mapping to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="MentionMappingEntity.Id"/> value assigned to the new mapping.
        /// </returns>
        Task<ulong> CreateAsync(MentionMappingCreationData data);

        /// <summary>
        /// Modifies an existing mapping within the repository.
        /// </summary>
        /// <param name="roleId">The <see cref="MentionMappingEntity.RoleId"/> value of the mapping to be modified.</param>
        /// <param name="updateAction">An action that describes how to modify the mapping.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the modification operation was successful.
        /// </returns>
        Task<bool> TryUpdateAsync(ulong roleId, Action<MentionMappingMutationData> updateAction);

        /// <summary>
        /// Retrieves information about a claim mapping based on the role it is associated with.
        /// </summary>
        /// <param name="roleId">The <see cref="MentionMappingEntity.RoleId"/> value of the mapping to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested mapping, or null if no such mapping exists.
        /// </returns>
        Task<MentionMappingSummary> ReadAsync(ulong roleId);
    }

    public class MentionMappingRepository : RepositoryBase, IMentionMappingRepository
    {
        /// <summary>
        /// Creates a new <see cref="MentionMappingRepository"/>.
        /// </summary>
        public MentionMappingRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginUpdateTransactionAsync()
            => _updateTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginDeleteTransactionAsync()
            => _deleteTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<ulong> CreateAsync(MentionMappingCreationData data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.MentionMappings.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            return entity.RoleId;
        }

        /// <inheritdoc />
        public Task<MentionMappingSummary> ReadAsync(ulong roleId)
            => ModixContext.MentionMappings.AsNoTracking()
                .AsExpandable()
                .Include(x => x.Role)
                .Include(x => x.MinimumRank)
                .Select(MentionMappingSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Role.Id == roleId);

        /// <inheritdoc />
        public async Task<bool> TryUpdateAsync(ulong roleId, Action<MentionMappingMutationData> updateAction)
        {
            if (updateAction is null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.MentionMappings
                .FirstOrDefaultAsync(x => x.RoleId == roleId);

            if (entity is null)
                return false;

            var data = MentionMappingMutationData.FromEntity(entity);
            updateAction(data);
            data.ApplyTo(entity);

            ModixContext.Update(entity);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _updateTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _deleteTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
