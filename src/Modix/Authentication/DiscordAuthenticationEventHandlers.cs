using System;
using System.Linq;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

using Modix.Models;

namespace Modix.Authentication
{
    public static class DiscordAuthenticationEventHandlers
    {
        public static Task OnCreatingTicket(
            OAuthCreatingTicketContext context)
        {
            var user = ModixUser.FromClaimsPrincipal(context.Principal);

            var userIsInGuilds = context.HttpContext.RequestServices.GetRequiredService<DiscordSocketClient>()
                .Guilds.Any(x => x.GetUser(user.UserId) is { });

            if (!userIsInGuilds)
                context.Fail("You must be a member of one of Modix's servers to log in.");

            return Task.CompletedTask;
        }

        public static Task OnRemoteFailure(
            RemoteFailureContext context)
        {
            context.Response.Redirect("/error");
            var errorMessage = context.Failure.Message;

            //Generic oauth error
            if (errorMessage == "access_denied")
                errorMessage = "There was a problem authenticating via OAuth. Try again later.";

            context.Response.Cookies.Append("Error", errorMessage, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(1) });
            context.HandleResponse();

            return Task.CompletedTask;
        }
    }
}
