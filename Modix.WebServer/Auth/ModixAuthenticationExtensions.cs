using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models;
using Newtonsoft.Json;
using Serilog;

namespace Modix.WebServer.Auth
{
    public static class ModixAuthenticationExtensions
    {
        public static AuthenticationBuilder AddModix(this AuthenticationBuilder builder, ModixConfig config)
        {
            return builder.AddOAuth<DiscordAuthenticationOptions, ModixAuthenticationHandler>(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ClaimActions.MapJsonKey(claimType: "avatarHash", jsonKey: "avatar");

                options.ClientId = config.DiscordClientId;
                options.ClientSecret = config.DiscordClientSecret;
                options.Scope.Add("identify");
                options.Scope.Add("guilds");

                options.Events.OnRemoteFailure = context =>
                {
                    context.Response.Redirect("/error");
                    string errorMessage = context.Failure.Message;

                    //Generic oauth error
                    if (errorMessage == "access_denied")
                    {
                        errorMessage = "There was a problem authenticating via OAuth. Try again later.";
                    }

                    context.Response.Cookies.Append("Error", errorMessage, new CookieOptions { Expires = DateTimeOffset.Now.AddHours(1) });
                    context.HandleResponse();

                    return Task.CompletedTask;
                };
            });
        }
    }
}
