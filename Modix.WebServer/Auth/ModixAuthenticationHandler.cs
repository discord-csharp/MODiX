using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modix.WebServer.Models;

namespace Modix.WebServer.Auth
{
    public class ModixAuthenticationHandler : DiscordAuthenticationHandler
    {
        private readonly DiscordSocketClient _client;

        public ModixAuthenticationHandler(IOptionsMonitor<DiscordAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, DiscordSocketClient client)
            : base(options, logger, encoder, clock)
        {
            _client = client;
        }

        // Not sure what the best way to pass exception message through,
        // so I settled for static since it's unlikely to see any changes to this at runtime.
        // TODO: Review this.
        public static string WebAuthenticationErrorMessage { get; set; }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
            AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var baseResult = await base.CreateTicketAsync(identity, properties, tokens);
            var result = DiscordUser.FromClaimsPrincipal(baseResult.Principal);
            var guild = _client.Guilds.FirstOrDefault();

            await guild?.DownloadUsersAsync();

            if (guild?.GetUser(result.UserId) == null)
                throw new UnauthorizedAccessException(WebAuthenticationErrorMessage);

            return baseResult;
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            try
            {
                return await base.HandleRemoteAuthenticateAsync();
            }
            catch (UnauthorizedAccessException e)
            {
                return HandleRequestResult.Fail(e);
            }
        }
    }
}