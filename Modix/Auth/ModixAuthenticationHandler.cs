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
using Modix.Data.Models.Core;
using Modix.Models;

namespace Modix.Auth
{
    public class ModixAuthenticationHandler : DiscordAuthenticationHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptions<ModixConfig> _config;

        public ModixAuthenticationHandler(IOptionsMonitor<DiscordAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,
            DiscordSocketClient client, IOptions<ModixConfig> config)
            : base(options, logger, encoder, clock)
        {
            _client = client;
            _config = config;

            options.CurrentValue.ClientId = config.Value.DiscordClientId;
            options.CurrentValue.ClientSecret = config.Value.DiscordClientSecret;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var baseResult = await base.CreateTicketAsync(identity, properties, tokens);
            var result = ModixUser.FromClaimsPrincipal(baseResult.Principal);

            var userWasfound = _client.Guilds.Any(d => d.GetUser(result.UserId) != null);

            if (!userWasfound)
            {
                throw new UnauthorizedAccessException("You must be a member of one of Modix's servers to log in.");
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
