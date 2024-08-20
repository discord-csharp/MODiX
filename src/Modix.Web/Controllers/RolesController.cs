using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Controllers;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/roles")]
[ApiController]
[Authorize]
public class RolesController : ModixController
{
    public RolesController(DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
    }

    [HttpGet]
    public async Task<Dictionary<ulong, RoleInformation>> GetRoles()
    {
        return UserGuild.Roles
            .Select(x => new RoleInformation(x.Id, x.Name, x.Color.ToString()))
            .ToDictionary(x => x.Id);
    }
}
