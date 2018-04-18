using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize]
    public class ModixController : Controller
    {
        //TODO: Un-hardcode this
        private const ulong _staffRoleId = 268470383571632128;

        protected DiscordSocketClient _client;
        private SocketGuildUser _socketUser;

        public DiscordUser DiscordUser { get; private set; }       
        public SocketGuildUser SocketUser => _socketUser ?? (_socketUser = _client.Guilds.First().GetUser(DiscordUser.UserId));
        public bool IsStaff => SocketUser.Roles.Any(d => d.Id == _staffRoleId) || SocketUser.Guild.Owner == SocketUser;

        public ModixController(DiscordSocketClient client)
        {
            _client = client;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (HttpContext.User != null)
            {
                DiscordUser = DiscordUser.FromClaimsPrincipal(HttpContext.User);

                if (SocketUser == null)
                {
                    context.Result = new RedirectResult("/api/logout");
                }

                DiscordUser.UserRole = (IsStaff ? UserRole.Staff : UserRole.Member);
            }

            await next();
        }
    }
}
