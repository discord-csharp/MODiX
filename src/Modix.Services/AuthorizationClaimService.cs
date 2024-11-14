using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data;
using Modix.Data.Models.Core;

namespace Modix.Services;

public class AuthorizationClaimService(ModixContext db)
{
    public async Task<IReadOnlyCollection<AuthorizationClaim>> GetClaimsForUser(ulong userId)
    {
        return await db.Set<ClaimMappingEntity>()
            .Where(x => x.UserId == userId)
            .Where(x => x.Type == ClaimMappingType.Granted)
            .Where(x => x.DeleteActionId == null)
            .Select(x => x.Claim)
            .ToListAsync();
    }
}
