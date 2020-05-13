using System;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;

using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Diagnostics
{
    [ServiceBinding(ServiceLifetime.Transient)]
    public class DiscordRestAvailabilityEndpoint
        : IAvailabilityEndpoint
    {
        public DiscordRestAvailabilityEndpoint(
            IDiscordRestClient discordClient)
        {
            _discordClient = discordClient;
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

                var user = await _discordClient.GetUserAsync(_discordClient.CurrentUser.Id, options);

                return user.Id == _discordClient.CurrentUser.Id;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private readonly IDiscordRestClient _discordClient;
    }
}
