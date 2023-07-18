using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="DesignatedChannelMappingEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IDesignatedChannelMappingRepository
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
        /// Creates a new mapping withn the repository.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginCreateTransactionAsync"/>.
        /// </summary>
        /// <param name="data">The data for the mapping to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="DesignatedChannelMappingEntity.Id"/> value assigned to the new mapping.
        /// </returns>
        Task<long> CreateAsync(DesignatedChannelMappingCreationData data);

        /// <summary>
        /// Checks whether any mappings exist within the repository, according to an arbitrary set of criteria.
        /// </summary>
        /// <param name="criteria">A set of criteria defining the mappings to check for.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching mappings were found.
        /// </returns>
        Task<bool> AnyAsync(
            DesignatedChannelMappingSearchCriteria criteria,
            CancellationToken cancellationToken);

        /// <summary>
        /// Searches the repository for mapped <see cref="DesignatedChannelMappingEntity.ChannelId"/> values, based on an arbitrary set of criteria
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting ID values to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching values have been retrieved.</returns>
        Task<IReadOnlyCollection<ulong>> SearchChannelIdsAsync(DesignatedChannelMappingSearchCriteria searchCriteria);

        /// <summary>
        /// Searches the repository for mappings, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="DesignatedChannelMappingBrief"/> records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> SearchBriefsAsync(DesignatedChannelMappingSearchCriteria searchCriteria);

        /// <summary>
        /// Marks mappings within the repository as deleted, based on a given set of search criteria.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginDeleteTransactionAsync"/>.
        /// </summary>
        /// <param name="criteria">A set of criteria defining the mappings to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the mapping.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the total number of mappings that were deleted, based on the given criteria.
        /// </returns>
        Task<int> DeleteAsync(DesignatedChannelMappingSearchCriteria criteria, ulong deletedById);

        /// <summary>
        /// Marks an existing mapping as deleted, based on its ID.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginDeleteTransactionAsync"/>.
        /// </summary>
        /// <param name="mappingId">The <see cref="DesignatedChannelMappingEntity.Id"/> value of the mapping to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the mapping.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation was successful (I.E. whether the specified mapping could be found).
        /// </returns>
        Task<bool> TryDeleteAsync(long mappingId, ulong deletedById);
    }

    /// <inheritdoc />
    public class DesignatedChannelMappingRepository : RepositoryBase, IDesignatedChannelMappingRepository
    {
        /// <summary>
        /// Creates a new <see cref="DesignatedChannelMappingRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public DesignatedChannelMappingRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginDeleteTransactionAsync()
            => _deleteTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(DesignatedChannelMappingCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.Set<DesignatedChannelMappingEntity>().AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.DesignatedChannelMappingId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(
                DesignatedChannelMappingSearchCriteria criteria,
                CancellationToken cancellationToken)
            => ModixContext.Set<DesignatedChannelMappingEntity>().AsNoTracking()
                .FilterBy(criteria)
                .AnyAsync(cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ulong>> SearchChannelIdsAsync(DesignatedChannelMappingSearchCriteria searchCriteria)
            => await ModixContext.Set<DesignatedChannelMappingEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .Select(x => x.ChannelId)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> SearchBriefsAsync(DesignatedChannelMappingSearchCriteria searchCriteria)
            => await ModixContext.Set<DesignatedChannelMappingEntity>().AsNoTracking()
                .FilterBy(searchCriteria)
                .AsExpandable()
                .Select(DesignatedChannelMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<int> DeleteAsync(DesignatedChannelMappingSearchCriteria criteria, ulong deletedById)
        {
            var entities = await ModixContext.Set<DesignatedChannelMappingEntity>()
                .Where(x => x.DeleteActionId == null)
                .FilterBy(criteria)
                .ToArrayAsync();

            foreach (var entity in entities)
                DoEntityDelete(entity, deletedById);

            await ModixContext.SaveChangesAsync();

            return entities.Length;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long mappingId, ulong deletedById)
        {
            var entity = await ModixContext.Set<DesignatedChannelMappingEntity>()
                .Where(x => x.Id == mappingId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            DoEntityDelete(entity, deletedById);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();

        private static readonly RepositoryTransactionFactory _deleteTransactionFactory
            = new RepositoryTransactionFactory();

        private void DoEntityDelete(DesignatedChannelMappingEntity entity, ulong deletedById)
            => entity.DeleteAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.DesignatedChannelMappingDeleted,
                Created = DateTimeOffset.UtcNow,
                CreatedById = deletedById,
                DesignatedChannelMappingId = entity.Id,
                GuildId = entity.GuildId
            };
    }
}
