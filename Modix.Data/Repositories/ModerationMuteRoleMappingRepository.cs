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
    public class ModerationMuteRoleMappingRepository : RepositoryBase, IModerationMuteRoleMappingRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ModerationMuteRoleMappingRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionProvider.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task<long> CreateAsync(ModerationMuteRoleMappingCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.ModerationMuteRoleMappings.AddAsync(entity);
            await ModixContext.SaveChangesAsync();

            entity.CreateAction.ModerationMuteRoleMappingId = entity.Id;
            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ModerationMuteRoleMappingBrief>> SearchBriefsAsync(ModerationMuteRoleMappingSearchCriteria criteria)
            => await ModixContext.ModerationMuteRoleMappings.AsNoTracking()
                .FilterModerationMuteRoleMappingsBy(criteria)
                .Select(ModerationMuteRoleMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long mappingId, ulong deletedById)
        {
            var longDeletedById = (long)deletedById;

            var entity = await ModixContext.ModerationMuteRoleMappings
                .Where(x => x.Id == mappingId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            entity.DeleteAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.ModerationMuteRoleMappingDeleted,
                Created = DateTimeOffset.Now,
                CreatedById = longDeletedById,
                ModerationMuteRoleMappingId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionProvider
            = new RepositoryTransactionFactory();
    }

    internal static class ModerationMuteRoleMappingQueryableExtensions
    {
        public static IQueryable<ModerationMuteRoleMappingEntity> FilterModerationMuteRoleMappingsBy(this IQueryable<ModerationMuteRoleMappingEntity> query, ModerationMuteRoleMappingSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longCreatedById = (long?)criteria?.CreatedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
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
