using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Diagnostics
{
    public sealed class HttpGetAvailabilityEndpoint
        : IAvailabilityEndpoint,
            IDisposable
    {
        public HttpGetAvailabilityEndpoint(
            string displayName,
            string url,
            IHttpClientFactory httpClientFactory)
        {
            _displayName = displayName;
            _url = url;

            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MODiX");
        }

        public string DisplayName
            => _displayName;

        public void Dispose()
            => _httpClient.Dispose();

        public async Task<bool> GetAvailabilityAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                return (await _httpClient.GetAsync(
                        _url,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken))
                    .IsSuccessStatusCode;
            }
            catch (Exception ex) when (
                    (ex is HttpRequestException)
                ||  (ex is TaskCanceledException))
            {
                return false;
            }
        }

        private readonly string _displayName;
        private readonly HttpClient _httpClient;
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
                .AddSingleton<IAvailabilityEndpoint>(serviceProvider => new HttpGetAvailabilityEndpoint("Github REST", "https://api.github.com", serviceProvider.GetRequiredService<IHttpClientFactory>()));
    }
}
