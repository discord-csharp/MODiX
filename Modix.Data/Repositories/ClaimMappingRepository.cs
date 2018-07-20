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
        public async Task<long> CreateAsync(ClaimMappingCreationData data)
        {
            using (await _createLock.LockAsync())
            {
                return await DoCreateAsync(data);
            }
        }

        /// <inheritdoc />
        public async Task<long?> TryCreateAsync(ClaimMappingCreationData data)
        {
            using (await _createLock.LockAsync())
            {
                if (await ModixContext.ClaimMappings.AsNoTracking()
                    .FilterClaimMappingsBy(new ClaimMappingSearchCriteria()
                    {
                        Types = new [] { data.Type },
                        GuildId = data.GuildId,
                        RoleIds = (data.RoleId == null) ? null : new [] { data.RoleId.Value },
                        UserId = data.UserId,
                        Claims = new [] { data.Claim },
                        IsRescinded = false
                    })
                    .AnyAsync())
                    return null;

                return await DoCreateAsync(data);
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
        public async Task<bool> TryRescindAsync(long claimMappingId, ulong rescindedById)
        {
            var longRescindedById = (long)rescindedById;

            if (await ModixContext.Users.AsNoTracking()
                .AnyAsync(x => x.Id == longRescindedById))
                return false;

            var entity = await ModixContext.ClaimMappings
                .Where(x => x.Id == claimMappingId)
                .FirstOrDefaultAsync();

            if ((entity == null) || (entity.RescindActionId != null))
                return false;

            entity.RescindAction = new ConfigurationActionEntity()
            {
                Type = ConfigurationActionType.ClaimMappingRescinded,
                Created = DateTimeOffset.Now,
                CreatedById = longRescindedById
            };

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private async Task<long> DoCreateAsync(ClaimMappingCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.ClaimMappings.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
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

            return query
                .FilterBy(
                    x => criteria.Types.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => x.GuildId == longGuildId,
                    longGuildId != null)
                .FilterBy(
                    x => longRoleIds.Contains(x.RoleId.Value),
                    longRoleIds?.Any() ?? false)
                .FilterBy(
                    x => x.UserId == longUserId,
                    longUserId != null)
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
                    x => x.RescindActionId.HasValue == criteria.IsRescinded.Value,
                    criteria?.IsRescinded != null);
        }
    }
}
