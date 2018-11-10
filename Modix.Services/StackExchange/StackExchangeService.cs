using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Modix.Services.StackExchange
{
    public class StackExchangeService
    {
        private static HttpClient HttpClient => new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });

        private string _apiReferenceUrl =
            "http://api.stackexchange.com/2.2/search/advanced" +
            "?key={0}" +
            "&order=desc" +
            "&sort=votes" +
            "&filter=default";

        public async Task<StackExchangeResponse> GetStackExchangeResultsAsync(string token, string phrase, string site, string tags)
        {
            _apiReferenceUrl = string.Format(_apiReferenceUrl, token);
            phrase = Uri.EscapeDataString(phrase);
            site = Uri.EscapeDataString(site);
            tags = Uri.EscapeDataString(tags);
            var query = _apiReferenceUrl += $"&site={site}&tags={tags}&q={phrase}";

            var response = await HttpClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                throw new WebException("Something failed while querying the Stack Exchange API.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StackExchangeResponse>(jsonResponse);
        }
    }
}
