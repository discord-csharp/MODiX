using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modix.Services.Diagnostics
{
    public sealed class PingLatencyEndpoint
        : ILatencyEndpoint,
            IDisposable
    {
        public PingLatencyEndpoint(
            string displayName,
            string url,
            ILogger<PingLatencyEndpoint> logger)
        {
            _displayName = displayName;
            _logger = logger;
            _url = url;

            _ping = new Ping();
        }

        public string DisplayName
            => _displayName;

        public void Dispose()
            => _ping.Dispose();

        public async Task<long?> GetLatencyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.Register(_ping.SendAsyncCancel);

            try
            {
                return (await _ping.SendPingAsync(_url))
                    .RoundtripTime;
            }
            catch (PingException ex)
            {
                _logger.LogWarning(ex, "Unable to ping endpoint: {Url}", _url);

                return null;
            }
        }

        private readonly string _displayName;
        private readonly ILogger _logger;
        private readonly Ping _ping;
        private readonly string _url;
    }

    [ServiceConfigurator]
    public class PingLatencyEndpointServiceConfigurator
        : IServiceConfigurator
    {
        public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration)
            => services
                .AddSingleton<ILatencyEndpoint>(serviceProvider => new PingLatencyEndpoint("Google",     "8.8.8.8", serviceProvider.GetRequiredService<ILogger<PingLatencyEndpoint>>()))
                .AddSingleton<ILatencyEndpoint>(serviceProvider => new PingLatencyEndpoint("Cloudflare", "1.1.1.1", serviceProvider.GetRequiredService<ILogger<PingLatencyEndpoint>>()));
    }
}
