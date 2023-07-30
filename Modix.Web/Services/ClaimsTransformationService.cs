using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Modix.Services.Core;

namespace Modix.Web.Services;

public class ClaimsTransformationService : IClaimsTransformation
{
    private readonly IAuthorizationService _authorizationService;
    private readonly DiscordUserService _discordUserService;

    public ClaimsTransformationService(IAuthorizationService authorizationService, DiscordUserService discordUserService)
    {
        _authorizationService = authorizationService;
        _discordUserService = discordUserService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.Identity?.IsAuthenticated ?? false)
            return principal;

        var userId = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!ulong.TryParse(userId, out var userSnowflake))
            return principal;

        var newPrincipal = principal.Clone();
        if (newPrincipal.Identity is not ClaimsIdentity claimsIdentity)
            return principal;

        // TODO: Get selected guild from cookie
        var currentGuild = _discordUserService.GetUserGuild();
        var currentUser = currentGuild.GetUser(userSnowflake);

        var claims = (await _authorizationService.GetGuildUserClaimsAsync(currentUser))
            .Select(d => new Claim(ClaimTypes.Role, d.ToString()));

        claimsIdentity.AddClaims(claims);

        return newPrincipal;
    }
}
