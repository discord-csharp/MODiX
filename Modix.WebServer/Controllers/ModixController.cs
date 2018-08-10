using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.WebServer.Auth;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize]
    public class ModixController : Controller
    {
        protected DiscordSocketClient DiscordSocketClient { get; private set; }
        protected ModixUser ModixUser { get; private set; }
        protected SocketGuildUser SocketUser { get; private set; }

        private Services.Core.IAuthorizationService _modixAuth;

        public ModixController(DiscordSocketClient client, Services.Core.IAuthorizationService modixAuth)
        {
            DiscordSocketClient = client;
            _modixAuth = modixAuth;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (HttpContext.User == null) { await next(); return; }

            ModixUser = ModixUser.FromClaimsPrincipal(HttpContext.User);

            var guild = DiscordSocketClient.Guilds.First();
            SocketUser = guild.GetUser(ModixUser.UserId);

            if (SocketUser == null) { await next(); return; }

            await AssignClaims();

            await next();
        }

        protected async Task AssignClaims()
        {
            await _modixAuth.OnAuthenticatedAsync(SocketUser);

            var claims = (await _modixAuth.GetGuildUserClaimsAsync(SocketUser))
                .Select(d => new Claim(ClaimTypes.Role, d.ToString()));

            (HttpContext.User.Identity as ClaimsIdentity).AddClaims(claims);
        }
    }
}
