using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

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
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching log channel mappings were found.
        /// </returns>
        Task<bool> AnyAsync(DesignatedChannelMappingSearchCriteria criteria);

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

            await ModixContext.DesignatedChannelMappings.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.DesignatedChannelMappingId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(DesignatedChannelMappingSearchCriteria criteria)
            => ModixContext.DesignatedChannelMappings.AsNoTracking()
                .FilterDesignatedChannelMappingsBy(criteria)
                .AnyAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<DesignatedChannelMappingBrief>> SearchBriefsAsync(DesignatedChannelMappingSearchCriteria criteria)
            => await ModixContext.DesignatedChannelMappings.AsNoTracking()
                .FilterDesignatedChannelMappingsBy(criteria)
                .Select(DesignatedChannelMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<int> DeleteAsync(DesignatedChannelMappingSearchCriteria criteria, ulong deletedById)
        {
            var entities = await ModixContext.DesignatedChannelMappings
                .FilterDesignatedChannelMappingsBy(criteria)
                .ToArrayAsync();

            foreach (var entity in entities)
                DoEntityDelete(entity, deletedById);

            await ModixContext.SaveChangesAsync();

            return entities.Length;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long mappingId, ulong deletedById)
        {
            var longDeletedById = (long)deletedById;

            var entity = await ModixContext.DesignatedChannelMappings
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
        {
            var longDeletedById = (long)deletedById;

            entity.DeleteAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.DesignatedChannelMappingDeleted,
                Created = DateTimeOffset.Now,
                CreatedById = longDeletedById,
                DesignatedChannelMappingId = entity.Id,
                GuildId = entity.GuildId
            };
        }
    }

    internal static class DesignatedChannelMappingQueryableExtensions
    {
        public static IQueryable<DesignatedChannelMappingEntity> FilterDesignatedChannelMappingsBy(this IQueryable<DesignatedChannelMappingEntity> query, DesignatedChannelMappingSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longChannelId = (long?)criteria?.ChannelId;
            var longCreatedById = (long?)criteria?.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => x.ChannelId == longChannelId,
                    longChannelId != null)
                .FilterBy(
                    x => x.ChannelDesignation == criteria.ChannelDesignation,
                    criteria.ChannelDesignation != null)
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange.Value.From,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange.Value.To,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById,
                    longCreatedById != null)
                .FilterBy(
                    x => (x.DeleteActionId != null) == criteria.IsDeleted,
                    criteria?.IsDeleted != null);
        }
    }
}
