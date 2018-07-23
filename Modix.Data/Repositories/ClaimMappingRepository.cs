using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Nito.AsyncEx;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
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
        public async Task<long?> TryCreateAsync(ClaimMappingCreationData data, ClaimMappingSearchCriteria criteria = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (await _createLock.LockAsync())
            {
                if ((criteria != null) && await ModixContext.ClaimMappings.AsNoTracking()
                    .FilterClaimMappingsBy(criteria).AnyAsync())
                {
                    return null;
                }

                var entity = data.ToEntity();

                await ModixContext.ClaimMappings.AddAsync(entity);
                await ModixContext.SaveChangesAsync();

                entity.CreateAction.ClaimMappingId = entity.Id;
                await ModixContext.SaveChangesAsync();

                return entity.Id;
            }
        }

        /// <inheritdoc />
        public Task<ClaimMappingSummary> ReadAsync(long roleClaimId)
            => ModixContext.ClaimMappings.AsNoTracking()
                .Select(ClaimMappingSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Id == roleClaimId);

        /// <inheritdoc />
        public Task<bool> AnyAsync(ulong guildId)
        {
            var longGuildId = (long)guildId;

            return ModixContext.ClaimMappings.AsNoTracking()
                .AnyAsync(x => x.GuildId == longGuildId);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<long>> SearchIdsAsync(ClaimMappingSearchCriteria criteria)
            => await ModixContext.ClaimMappings.AsNoTracking()
                .FilterClaimMappingsBy(criteria)
                .Select(x => x.Id)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ClaimMappingBrief>> SearchBriefsAsync(ClaimMappingSearchCriteria criteria)
            => await ModixContext.ClaimMappings.AsNoTracking()
                .FilterClaimMappingsBy(criteria)
                .Select(ClaimMappingBrief.FromEntityProjection)
                .ToArrayAsync();

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync(long claimMappingId, ulong rescindedById)
        {
            var longRescindedById = (long)rescindedById;

            if (await ModixContext.Users.AsNoTracking()
                .AnyAsync(x => x.Id == longRescindedById))
                return false;

            var entity = await ModixContext.ClaimMappings
                .Where(x => x.Id == claimMappingId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.DeleteActionId != null))
                return false;

            entity.DeleteAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.ClaimMappingDeleted,
                Created = DateTimeOffset.Now,
                CreatedById = longRescindedById,
                ClaimMappingId = entity.Id
            };
            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly AsyncLock _createLock
            = new AsyncLock();
    }

    internal static class ClaimMappingQueryableExtensions
    {
        internal static IQueryable<ClaimMappingEntity> FilterClaimMappingsBy(this IQueryable<ClaimMappingEntity> query, ClaimMappingSearchCriteria criteria)
        {
            var longGuildId = (long?)criteria?.GuildId;
            var longRoleIds = criteria?.RoleIds?.Select(x => (long)x).ToArray();
            var longUserId = (long?)criteria?.UserId;
            var longCreatedById = (long?)criteria?.CreatedById;

            var anyRoleIds = longRoleIds?.Any() ?? false;

            return query
                .FilterBy(
                    x => criteria.Types.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => longRoleIds.Contains(x.RoleId.Value) || (x.UserId == longUserId),
                    anyRoleIds && (longUserId != null))
                .FilterBy(
                    x => longRoleIds.Contains(x.RoleId.Value),
                    anyRoleIds && (longUserId == null))
                .FilterBy(
                    x => (x.UserId == longUserId),
                    !anyRoleIds && (longUserId == null))
                .FilterBy(
                    x => criteria.Claims.Contains(x.Claim),
                    criteria?.Claims?.Any() ?? false)
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange.Value.From.Value,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange.Value.To.Value,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById,
                    longCreatedById != null)
                .FilterBy(
                    x => (x.DeleteActionId != null) == criteria.IsDeleted.Value,
                    criteria?.IsDeleted != null);
        }
    }
}
