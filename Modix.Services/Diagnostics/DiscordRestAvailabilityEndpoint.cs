using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modix.Services.Diagnostics
{
    [ServiceBinding(ServiceLifetime.Transient)]
    public class DiscordRestAvailabilityEndpoint
        : IAvailabilityEndpoint
    {
        public DiscordRestAvailabilityEndpoint(
            DiscordRestClient discordClient,
            ILogger<DiscordRestAvailabilityEndpoint> logger)
        {
            _discordClient = discordClient;
            _logger = logger;
        }

        public string DisplayName
            => "Discord REST";

        public async Task<bool> GetAvailabilityAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var options = RequestOptions.Default.Clone();
                options.CancelToken = cancellationToken;

                await _discordClient.GetUserAsync(_discordClient.CurrentUser.Id, options);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "The Discord REST service appears to be unavailable");

                return false;
            }
        }

        private readonly DiscordRestClient _discordClient;
        private readonly ILogger _logger;
    }
}
