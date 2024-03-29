using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ClaimMappingEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IClaimMappingRepository
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
        /// Begins a new transaction to delete mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other delete transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginDeleteTransactionAsync();

        /// <summary>
        /// Creates a new claim mapping within the repository.
        /// </summary>
        /// <param name="data">The data for the mapping to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ClaimMappingEntity.Id"/> value assigned to the new mapping.
        /// </returns>
        Task<long> CreateAsync(ClaimMappingCreationData data);

        /// <summary>
        /// Retrieves information about a claim mapping, based on its ID.
        /// </summary>
        /// <param name="claimMappingId">The <see cref="ClaimMappingEntity.Id"/> value of the mapping to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested mapping, or null if no such mapping exists.
        /// </returns>
        Task<ClaimMappingSummary?> ReadAsync(long claimMappingId);

        /// <summary>
        /// Checks whether any claims exist, for an arbitrary set of criteria.
        /// </summary>
        /// <param name="criteria">A set of criteria defining the mappings to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching claim mappings were found.
        /// </returns>
        Task<bool> AnyAsync(ClaimMappingSearchCriteria criteria);

        /// <summary>
        /// Searches the repository for claim mapping id values, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="ClaimMappingEntity.Id"/> values to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching values have been retrieved.</returns>
        Task<IReadOnlyCollection<long>> SearchIdsAsync(ClaimMappingSearchCriteria criteria);

        /// <summary>
        /// Searches the repository for claim mapping information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="criteria">The criteria for selecting <see cref="ClaimMappingBrief"/> records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<ClaimMappingBrief>> SearchBriefsAsync(ClaimMappingSearchCriteria criteria);

        /// <summary>
        /// Marks an existing claim mapping as rescinded, based on its ID.
        /// </summary>
        /// <param name="claimMappingId">The <see cref="ClaimMappingEntity.Id"/> value of the mapping to be rescinded.</param>
        /// <param name="rescindedById">The <see cref="UserEntity.Id"/> value of the user that is rescinding the mapping.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified mapping could be found).
        /// </returns>
        Task<bool> TryDeleteAsync(long claimMappingId, ulong rescindedById);
    }

    /// <inheritdoc />
    public class ClaimMappingRepository : RepositoryBase, IClaimMappingRepository
    {
        /// <summary>
        /// Creates a new <see cref="ClaimMappingRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ClaimMappingRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginDeleteTransactionAsync()
            => _deleteTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(ClaimMappingCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<ClaimMappingEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.ClaimMappingId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<ClaimMappingSummary?> ReadAsync(long claimMappingId)
            => ModixContext.Set<ClaimMappingEntity>().AsNoTracking()
                .AsExpandable()
                .Select(ClaimMappingSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Id == claimMappingId);

        /// <inheritdoc />
        public Task<bool> AnyAsync(ClaimMappingSearchCriteria criteria)
            => ModixContext.Set<ClaimMappingEntity>().AsNoTracking()
                .FilterBy(criteria)
                .AnyAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<long>> SearchIdsAsync(ClaimMappingSearchCriteria criteria)
            => await ModixContext.Set<ClaimMappingEntity>().AsNoTracking()
                .FilterBy(criteria)
                .Select(x => x.Id)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ClaimMappingBrief>> SearchBriefsAsync(ClaimMappingSearchCriteria criteria)
            => await ModixContext.Set<ClaimMappingEntity>().AsNoTracking()
                .FilterBy(criteria)
                .AsExpandable()
                .Select(ClaimMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long claimMappingId, ulong rescindedById)
        {
            var entity = await ModixContext.Set<ClaimMappingEntity>()
                .Where(x => x.Id == claimMappingId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            entity.DeleteAction = new ConfigurationActionEntity()
            {
                GuildId = entity.GuildId,
                Type = ConfigurationActionType.ClaimMappingDeleted,
                Created = DateTimeOffset.UtcNow,
                CreatedById = rescindedById,
                ClaimMappingId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _deleteTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
