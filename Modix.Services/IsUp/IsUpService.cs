using System;
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

        protected IHttpClientFactory HttpClientFactory { get; }

        public IsUpService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        public async Task<IsUpResponse> GetIsUpResponseAsync([Summary("Url to get status of")]string url)
        {
            string apiQueryUrl = $"{_apiBaseURl}{url}";
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                apiQueryUrl = $"{_apiBaseURl}{uri.Host}";
            }

            var client = HttpClientFactory.CreateClient();
            var response = await client.GetAsync(apiQueryUrl);

            if (!response.IsSuccessStatusCode)
            {
                return new IsUpResponse()
                {
                    StatusString = response.ReasonPhrase
                };
            }

            var JsonResp = await response.Content.ReadAsStringAsync();
            var RespObj = JsonConvert.DeserializeObject<IsUpResponse>(JsonResp);
            RespObj.StatusString = response.ReasonPhrase;
            return RespObj;
        }
    }
}
