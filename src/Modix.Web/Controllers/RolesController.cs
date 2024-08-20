using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Web.Models;
using Modix.Web.Shared.Models.Common;

namespace Modix.Web.Controllers;

[Route("~/api/roles")]
[ApiController]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly DiscordSocketClient _discordSocketClient;

    public RolesController(DiscordSocketClient discordSocketClient)
    {
        _discordSocketClient = discordSocketClient;
    }

    [HttpGet]
    public async Task<Dictionary<ulong, RoleInformation>> GetRoles()
    {
        // TODO: Move this to a base class like ModixController?

        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];

        SocketGuild guildToSearch;
        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = _discordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = _discordSocketClient.Guilds.First();
        }

        return guildToSearch.Roles
            .Select(x => new RoleInformation(x.Id, x.Name, x.Color.ToString()))
            .ToDictionary(x => x.Id);
    }
}
