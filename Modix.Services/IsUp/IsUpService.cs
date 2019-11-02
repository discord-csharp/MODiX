using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;

namespace Modix.Services.IsUp
{
    public class IsUpService
    {
        private const string _apiBaseURl = "https://isitdown.site/api/v3/";

        public IHttpClientFactory HttpClientFactory { get; }

        public IsUpService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<IsUpResponse> GetIsUpResponseAsync([Summary("Url to get status of")]string url)
        {
            var apiQueryUrl = $"{_apiBaseURl}{url}";
            var client = HttpClientFactory.CreateClient();
            var response = await client.GetAsync(apiQueryUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var JsonResp = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IsUpResponse>(JsonResp);
        }
    }
}
