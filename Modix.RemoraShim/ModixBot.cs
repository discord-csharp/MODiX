using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Remora.Discord.Gateway;
using Remora.Results;

namespace Modix.RemoraShim
{
    internal class ModixBot
        : BackgroundService
    {
        public ModixBot(DiscordGatewayClient client)
            => _client = client;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var result = await _client.RunAsync(stoppingToken);
            if (!result.IsSuccess)
                throw new Exception(result.Error.Message, (result.Error as ExceptionError)?.Exception);
        }

        private readonly DiscordGatewayClient _client;
    }
}
