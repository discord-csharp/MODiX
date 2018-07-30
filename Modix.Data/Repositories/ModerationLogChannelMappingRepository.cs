using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class ModerationLogChannelMappingRepository : RepositoryBase, IModerationLogChannelMappingRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ModerationLogChannelMappingRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginDeleteTransactionAsync()
            => _deleteTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(ModerationLogChannelMappingCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.ModerationLogChannelMappings.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.ModerationLogChannelMappingId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> AnyAsync(ModerationLogChannelMappingSearchCriteria criteria)
            => ModixContext.ModerationLogChannelMappings.AsNoTracking()
                .FilterModerationLogChannelMappingsBy(criteria)
                .AnyAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ModerationLogChannelMappingBrief>> SearchBriefsAsync(ModerationLogChannelMappingSearchCriteria criteria)
            => await ModixContext.ModerationLogChannelMappings.AsNoTracking()
                .FilterModerationLogChannelMappingsBy(criteria)
                .Select(ModerationLogChannelMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<int> DeleteAsync(ModerationLogChannelMappingSearchCriteria criteria, ulong deletedById)
        {
            var entities = await ModixContext.ModerationLogChannelMappings
                .FilterModerationLogChannelMappingsBy(criteria)
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

            var entity = await ModixContext.ModerationLogChannelMappings
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

        private void DoEntityDelete(ModerationLogChannelMappingEntity entity, ulong deletedById)
        {
            var longDeletedById = (long)deletedById;

            entity.DeleteAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.ModerationLogChannelMappingDeleted,
                Created = DateTimeOffset.Now,
                CreatedById = longDeletedById,
                ModerationLogChannelMappingId = entity.Id
            };
        }
    }

    internal static class ModerationLogChannelMappingQueryableExtensions
    {
        public static IQueryable<ModerationLogChannelMappingEntity> FilterModerationLogChannelMappingsBy(this IQueryable<ModerationLogChannelMappingEntity> query, ModerationLogChannelMappingSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longLogChannelId = (long?)criteria?.LogChannelId;
            var longCreatedById = (long?)criteria?.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => x.LogChannelId == longLogChannelId,
                    longLogChannelId != null)
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
