using AspNet.Security.OAuth.Discord;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Modix.Web.Shared.Models;

namespace Modix.Authentication
{
    public static class DiscordAuthenticationSetup
    {
        public static AuthenticationBuilder AddDiscordAuthentication(
            this AuthenticationBuilder builder)
        {
            return builder.AddOAuth<DiscordAuthenticationOptions, DiscordAuthenticationHandler>(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Events = new OAuthEvents()
                {
                    OnCreatingTicket = DiscordAuthenticationEventHandlers.OnCreatingTicket,
                    OnRemoteFailure = DiscordAuthenticationEventHandlers.OnRemoteFailure
                };

                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ClaimActions.MapJsonKey(claimType: nameof(DiscordUser.AvatarHash), jsonKey: "avatar");
            });
        }
    }
}
