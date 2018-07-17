using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class RoleClaimRepository : RepositoryBase, IRoleClaimRepository
    {
        /// <summary>
        /// Creates a new <see cref="RoleClaimRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public RoleClaimRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task<long> CreateAsync(RoleClaimCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.RoleClaims.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(ulong guildId, ulong roleId, AuthorizationClaim claim)
        {
            var longGuildId = (long)guildId;
            var longRoleId = (long)roleId;

            return ModixContext.RoleClaims.AsNoTracking()
                .Where(x => x.GuildId == longGuildId)
                .Where(x => x.GuildId == longGuildId)
                .Where(x => x.Claim == claim)
                .Where(x => x.RescindActionId == null)
                .AnyAsync();
        }

        /// <inheritdoc />
        public Task<RoleClaimSummary> ReadAsync(long roleClaimId)
            => ModixContext.RoleClaims.AsNoTracking()
                .Select(RoleClaimSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Id == roleClaimId);

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> ReadClaimsAsync(ulong guildId, params ulong[] roleIds)
        {
            var longGuildId = (long)guildId;
            var longRoleIds = roleIds?.Cast<long>() ?? Array.Empty<long>();

            return await ModixContext.RoleClaims.AsNoTracking()
                .Where(x => x.GuildId == longGuildId)
                .Where(x => longRoleIds.Contains(x.RoleId))
                .Where(x => x.RescindActionId == null)
                .Select(x => x.Claim)
                .Distinct()
                .ToArrayAsync();
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(long roleClaimId, Action<RoleClaimMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.RoleClaims
                .Where(x => x.Id == roleClaimId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = RoleClaimMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.RescindActionId);

            await ModixContext.SaveChangesAsync();

            return true;
        }
    }
}
