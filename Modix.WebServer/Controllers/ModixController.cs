using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Modix.WebServer.Models;

namespace Modix.WebServer.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize]
    public class ModixController : Controller
    {
        protected DiscordUser DiscordUser { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.User != null)
            {
                DiscordUser = DiscordUser.FromClaimsPrincipal(HttpContext.User);
            }

            base.OnActionExecuting(context);
        }
    }
}
