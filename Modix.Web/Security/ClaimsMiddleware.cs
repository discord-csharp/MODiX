using System.Security.Claims;
using Discord.WebSocket;
using Modix.Services.Core;
using Modix.Web.Services;

namespace Modix.Web.Security;

public class ClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public ClaimsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService, DiscordSocketClient discordClient)
    {
        var userId = context.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!ulong.TryParse(userId, out var userSnowflake))
        {
            await _next(context);
            return;
        }

        var selectedGuild = context.Request.Cookies[CookieService.GuildCookieKey];
        _ = ulong.TryParse(selectedGuild, out var selectedGuildId);

        if (context.User.Identity is not ClaimsIdentity claimsIdentity)
        {
            await _next(context);
            return;
        }

        var currentGuild = discordClient.GetGuild(selectedGuildId) ?? discordClient.Guilds.First();
        var currentUser = currentGuild.GetUser(userSnowflake);

        var claims = (await authorizationService.GetGuildUserClaimsAsync(currentUser))
            .Select(d => new Claim(ClaimTypes.Role, d.ToString()));

        claimsIdentity.AddClaims(claims);

        await _next(context);
    }
}
