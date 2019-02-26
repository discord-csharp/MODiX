using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modix.Services.StackExchange
{
    public class StackExchangeService
    {
        private string _apiReferenceUrl =
            "http://api.stackexchange.com/2.2/search/advanced" +
            "?key={0}" +
            "&order=desc" +
            "&sort=votes" +
            "&filter=default";

        public StackExchangeService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<StackExchangeResponse> GetStackExchangeResultsAsync(string token, string phrase, string site, string tags)
        {
            _apiReferenceUrl = string.Format(_apiReferenceUrl, token);
            phrase = Uri.EscapeDataString(phrase);
            site = Uri.EscapeDataString(site);
            tags = Uri.EscapeDataString(tags);
            var query = _apiReferenceUrl += $"&site={site}&tags={tags}&q={phrase}";

            var client = HttpClientFactory.CreateClient("StackExchangeClient");

            var response = await client.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the Stack Exchange API.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StackExchangeResponse>(jsonResponse);
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
