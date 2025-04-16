using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modix.Web.Shared.Models;

namespace Modix.Web.Controllers;

[Route("~/api/guild")]
[ApiController]
[Authorize]
public class GuildController : ModixController
{
    public GuildController(DiscordSocketClient discordSocketClient, Modix.Services.Core.IAuthorizationService authorizationService)
        : base(discordSocketClient, authorizationService)
    {
    }

    [HttpGet("guildoptions")]
    public GuildOption[] GuildOptions()
    {
        return DiscordSocketClient
            .Guilds
            .Where(d => d.GetUser(SocketUser?.Id ?? 0) != null)
            .Select(d => new GuildOption(d.Id, d.Name, d.IconUrl))
            .ToArray();
    }
}
