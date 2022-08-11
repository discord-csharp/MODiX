using System;
using System.Threading;
using System.Threading.Tasks;

using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Diagnostics
{
    [ServiceBinding(ServiceLifetime.Transient)]
    public class DiscordGatewayLatencyEndpoint
        : ILatencyEndpoint
    {
        public DiscordGatewayLatencyEndpoint(
            DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        public string DisplayName
            => "Discord Gateway";

        public Task<long?> GetLatencyAsync(
                CancellationToken cancellationToken)
            => Task.FromResult<long?>(Convert.ToInt64(_discordClient.Latency));

        private readonly DiscordSocketClient _discordClient;
    }
}
