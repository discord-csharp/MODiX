using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Diagnostics
{
    public sealed class PingLatencyEndpoint
        : ILatencyEndpoint,
            IDisposable
    {
        public PingLatencyEndpoint(
            string displayName,
            string url)
        {
            _displayName = displayName;
            _url = url;

            _ping = new Ping();
        }

        public string DisplayName
            => _displayName;

        public void Dispose()
            => _ping.Dispose();

        public async Task<long> GetLatencyAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.Register(_ping.SendAsyncCancel);

            var reply = await _ping.SendPingAsync(_url);

            return reply.RoundtripTime;
        }

        private readonly string _displayName;
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
                .AddSingleton<ILatencyEndpoint>(serviceProvider => new PingLatencyEndpoint("Google",     "8.8.8.8"))
                .AddSingleton<ILatencyEndpoint>(serviceProvider => new PingLatencyEndpoint("Cloudflare", "1.1.1.1"));
    }
}
