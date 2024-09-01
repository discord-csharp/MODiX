using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Models.Core;
using Modix.Web.Shared.Models.Configuration;

namespace Modix.Web.Controllers;

[Route("~/api/config/claims")]
[ApiController]
[Authorize(Roles = nameof(AuthorizationClaim.AuthorizationConfigure))]
public class ClaimsController : ModixController
{
    private readonly IClaimMappingRepository _claimMappingRepository;

    public ClaimsController(IClaimMappingRepository claimMappingRepository, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _claimMappingRepository = claimMappingRepository;
    }

    [HttpGet]
    public async Task<IEnumerable<ClaimMappingData>> GetMappedClaimsAsync()
    {
        var mappedClaims = await _claimMappingRepository.SearchBriefsAsync(new ClaimMappingSearchCriteria
        {
            IsDeleted = false,
            GuildId = UserGuild.Id
        });

        return mappedClaims
            .Select(x => new ClaimMappingData(x.RoleId, x.Claim, x.Type));
    }

    [HttpPatch("{roleId}/{authorizationClaim}/{claimMappingType?}")]
    public async Task ModifyClaimMappingAsync(ulong roleId, AuthorizationClaim authorizationClaim, ClaimMappingType? claimMappingType)
    {
        await ModixAuth.ModifyClaimMappingAsync(roleId, authorizationClaim, claimMappingType);
    }
}
