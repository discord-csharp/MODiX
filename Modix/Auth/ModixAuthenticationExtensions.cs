using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;

namespace Modix.Auth
{
    public static class ModixAuthenticationExtensions
    {
        public static AuthenticationBuilder AddModixAuth(this AuthenticationBuilder builder)
        {
            return builder.AddOAuth<DiscordAuthenticationOptions, ModixAuthenticationHandler>(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ClaimActions.MapJsonKey(claimType: "avatarHash", jsonKey: "avatar");
                options.Scope.Add("identify");
                //options.Scope.Add("guilds");

                options.Events.OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/error");
                    var errorMessage = context.Failure.Message;

                    //Generic oauth error
                    if (errorMessage == "access_denied")
                    {
                        errorMessage = "There was a problem authenticating via OAuth. Try again later.";
                    }

                    context.Response.Cookies.Append("Error", errorMessage, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(1) });
                    context.HandleResponse();

                    return Task.CompletedTask;
                };
            });
        }
    }
}
