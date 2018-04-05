using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.WebSocket;
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
        private const ulong _staffRoleId = 371437486225752066;

        protected DiscordSocketClient _client;
        private SocketGuildUser _socketUser;

        public DiscordUser DiscordUser { get; private set; }       
        public SocketGuildUser SocketUser => _socketUser ?? (_socketUser = _client.Guilds.First().GetUser(DiscordUser.UserId));
        public bool IsStaff => SocketUser.Roles.Any(d => d.Id == _staffRoleId);

        public ModixController(DiscordSocketClient client)
        {
            _client = client;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.User != null)
            {
                DiscordUser = DiscordUser.FromClaimsPrincipal(HttpContext.User);
                DiscordUser.UserRole = (IsStaff ? UserRole.Staff : UserRole.Member);
            }

            base.OnActionExecuting(context);
        }
    }
}
