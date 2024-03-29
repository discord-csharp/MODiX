using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modix.Services.Diagnostics
{
    public sealed class HttpGetAvailabilityEndpoint
        : IAvailabilityEndpoint
    {
        public HttpGetAvailabilityEndpoint(
            string displayName,
            string url,
            IHttpClientFactory httpClientFactory,
            ILogger<HttpGetAvailabilityEndpoint> logger)
        {
            _displayName = displayName;
            _url = url;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string DisplayName
            => _displayName;

        public async Task<bool> GetAvailabilityAsync(
            CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MODiX");

            try
            {
                return (await httpClient.GetAsync(
                        _url,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken))
                    .IsSuccessStatusCode;
            }
            catch (Exception ex) when (
                    (ex is HttpRequestException)
                ||  (ex is TaskCanceledException))
            {
                _logger.LogWarning(ex, "An HTTP Endpoint ({Url}) appears to be unavailable", _url);

                return false;
            }
        }

        private readonly string _displayName;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly string _url;
    }

    [ServiceConfigurator]
    public class HttpGetAvailabilityEndpointConfigurator
        : IServiceConfigurator
    {
        public void ConfigureServices(
                IServiceCollection services,
                IConfiguration configuration)
            => services
                .AddTransient<IAvailabilityEndpoint>(serviceProvider => new HttpGetAvailabilityEndpoint(
                    "Github REST",
                    "https://api.github.com",
                    serviceProvider.GetRequiredService<IHttpClientFactory>(),
                    serviceProvider.GetRequiredService<ILogger<HttpGetAvailabilityEndpoint>>()));
    }
}
