using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.Models;

namespace Modix.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize]
    public class ModixController : Controller
    {
        protected DiscordSocketClient DiscordSocketClient { get; private set; }
        protected ModixUser ModixUser { get; private set; }
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
            if (!DiscordSocketClient.Guilds.Any()) { await next(); return; }
            if (HttpContext.User == null) { await next(); return; }

            //Parse the ID to look up socket user
            ModixUser = ModixUser.FromClaimsPrincipal(HttpContext.User);

            var guildCookie = Request.Cookies["SelectedGuild"];
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

            SocketUser = guildToSearch?.GetUser(ModixUser.UserId);

            if (SocketUser == null) { await next(); return; }

            await AssignClaims();

            //Do it again here to assign claims (this is very lazy of us)
            ModixUser = ModixUser.FromClaimsPrincipal(HttpContext.User);
            ModixUser.SelectedGuild = SocketUser.Guild.Id;

            await next();
        }

        protected async Task AssignClaims()
        {
            await ModixAuth.OnAuthenticatedAsync(SocketUser);

            var claims = (await ModixAuth.GetGuildUserClaimsAsync(SocketUser))
                .Select(d => new Claim(ClaimTypes.Role, d.ToString()));

            (HttpContext.User.Identity as ClaimsIdentity).AddClaims(claims);
        }
    }
}
