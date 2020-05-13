﻿using System;
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
            IDiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        public string DisplayName
            => "Discord Gateway";

        public Task<long> GetLatencyAsync(
                CancellationToken cancellationToken)
            => Task.FromResult(Convert.ToInt64(_discordClient.Latency));

        private readonly IDiscordSocketClient _discordClient;
    }
}
