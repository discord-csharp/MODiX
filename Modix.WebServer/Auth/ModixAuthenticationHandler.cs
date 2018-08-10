using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Newtonsoft.Json;

namespace Modix.WebServer.Auth
{
    public class ModixAuthenticationHandler : DiscordAuthenticationHandler
    {
        private DiscordSocketClient _client;

        public ModixAuthenticationHandler(IOptionsMonitor<DiscordAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            DiscordSocketClient client) 
            : base(options, logger, encoder, clock)
        {
            _client = client;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var baseResult = await base.CreateTicketAsync(identity, properties, tokens);
            var result = ModixUser.FromClaimsPrincipal(baseResult.Principal);
            var guild = _client.Guilds.FirstOrDefault();

            await guild?.DownloadUsersAsync();

            var foundUser = guild?.GetUser(result.UserId);

            if (foundUser == null)
            {
                throw new UnauthorizedAccessException("You must be a member of the Discord C# server to log in.");
            }

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
