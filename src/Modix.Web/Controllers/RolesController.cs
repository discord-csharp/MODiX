using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Modix.Controllers;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/roles")]
[ApiController]
[Authorize]
public class RolesController : ModixController
{
    private readonly IMemoryCache _memoryCache;

    public RolesController(IMemoryCache memoryCache, DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public Dictionary<ulong, RoleInformation> GetRoles()
    {
        return _memoryCache.GetOrCreate("roles", cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            return UserGuild.Roles
                .Select(x => new RoleInformation(x.Id, x.Name, x.Color.ToString()))
                .ToDictionary(x => x.Id);
        })!;
    }
}
