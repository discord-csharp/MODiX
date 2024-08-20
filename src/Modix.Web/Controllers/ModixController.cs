#nullable enable
using System.Security.Claims;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.Web.Models;

namespace Modix.Controllers;

[Authorize]
public class ModixController : Controller
{
    protected DiscordSocketClient DiscordSocketClient { get; private set; }
    protected SocketGuildUser SocketUser { get; private set; }
    protected SocketGuild UserGuild => SocketUser.Guild;

    protected Services.Core.IAuthorizationService ModixAuth { get; private set; }

    public ModixController(DiscordSocketClient client, Services.Core.IAuthorizationService modixAuth)
    {
        DiscordSocketClient = client;
        ModixAuth = modixAuth;
    }


    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!DiscordSocketClient.Guilds.Any())
            return;

        if (User is null)
        {
            await HttpContext.ChallengeAsync();
            return;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!ulong.TryParse(userId, out var userSnowflake))
        {
            await HttpContext.ChallengeAsync();
            return;
        }

        var guildCookie = Request.Cookies[CookieConstants.SelectedGuild];
        SocketGuild guildToSearch;

        if (!string.IsNullOrWhiteSpace(guildCookie))
        {
            var guildId = ulong.Parse(guildCookie);
            guildToSearch = DiscordSocketClient.GetGuild(guildId);
        }
        else
        {
            guildToSearch = DiscordSocketClient.Guilds.First();
        }

        SocketUser = guildToSearch.GetUser(userSnowflake);

        if (SocketUser is null)
        {
            await HttpContext.ChallengeAsync();
            return;
        }

        await ModixAuth.OnAuthenticatedAsync(SocketUser.Id, SocketUser.Guild.Id, [.. SocketUser.Roles.Select(x => x.Id) ]);

        await next();
    }
}
