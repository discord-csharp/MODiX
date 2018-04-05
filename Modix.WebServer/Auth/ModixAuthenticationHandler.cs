using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Modix.WebServer.Auth
{
    public class ModixAuthenticationHandler : DiscordAuthenticationHandler
    {
        public ModixAuthenticationHandler(IOptionsMonitor<DiscordAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var baseResult = await base.CreateTicketAsync(identity, properties, tokens);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{DiscordAuthenticationDefaults.UserInformationEndpoint}/guilds");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("An error occurred while retrieving the user's guilds: the remote server " +
                                "returned a {Status} response with the following payload: {Headers} {Body}.",
                                /* Status: */ response.StatusCode,
                                /* Headers: */ response.Headers.ToString(),
                                /* Body: */ await response.Content.ReadAsStringAsync());

                throw new HttpRequestException("An error occurred while retrieving the user profile.");
            }

            string content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<DiscordGuildListItem>>(content);

            //TODO: Un-hardcode this
            if (!result.Any(d => d.Id == 143867839282020352))
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
