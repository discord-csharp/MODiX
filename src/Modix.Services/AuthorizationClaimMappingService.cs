using System;
using System.Threading.Tasks;
using Modix.Data;
using Modix.Data.Models.Core;

namespace Modix.Services;

public class AuthorizationClaimMappingService(ModixContext db, IScopedSession scopedSession)
{
    public async Task AddClaimForRole(ulong guildId, ulong roleId, AuthorizationClaim claim)
    {
        var entity = new ClaimMappingEntity
        {
            GuildId = guildId,
            RoleId = roleId,
            Claim = claim,
            Type = ClaimMappingType.Granted,
            CreateAction = new ConfigurationActionEntity
            {
                GuildId = guildId,
                Type = ConfigurationActionType.ClaimMappingCreated,
                Created = DateTimeOffset.UtcNow,
                CreatedById = scopedSession.ExecutingUserId
            }
        };

        db.Add(entity);

        await db.SaveChangesAsync();
    }
}
