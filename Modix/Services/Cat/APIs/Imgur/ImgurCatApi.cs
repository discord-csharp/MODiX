using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace Modix.Services.Cat.APIs.Imgur
{
    public class ImgurCatApi : ICatApi
    {
        // This can be public
        private const string ClientId = "c482f6336b58ec4";
        private const string Url = "https://api.imgur.com/3/gallery/r/cats/page/";

        private readonly HttpClient _httpClient = new HttpClient();
        private static readonly List<string> LinkPool = new List<string>();

        public ImgurCatApi()
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {ClientId}");
        }

        public async Task<CatResponse> Fetch(CatMediaType type, CancellationToken cancellationToken)
        {
            string catUrl = null;

            try
            {
                // If we have any cat URLs in the pool, try to fetch those first
                if (LinkPool.Any())
                {
                    catUrl = GetCachedCat();
                }
                else
                {
                    catUrl = await GetCatFromApi(cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning("Failed fetching Imgur cat", ex.InnerException);
            }

            if(!string.IsNullOrWhiteSpace(catUrl))
                return new UrlCatResponse(catUrl);

            return new UrlCatResponse();
        }

        private async Task<string> GetCatFromApi(CancellationToken cancellationToken)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(Url, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var imgur = Deserialise(content);

                        // We may have succeeded in the response, but Imgur may not like us,
                        // check if Imgur has returned us a successful result
                        if (!imgur.Success)
                            return null;

                        // We have a caching mechanism in place, therefore attempt to get
                        // all links of the URLs and cache
                        var links = imgur.Images.Select(x => x.Link).ToList();

                        // We want to return the first link, since we wanted to fetch a
                        // URL to begin with, so remove the first link and return that
                        // to the user
                        var primaryLink = links.First();

                        links.Remove(primaryLink);

                        LinkPool.AddRange(links);

                        return primaryLink;
                    }

                    // If there is a bad result, empty string will be returned
                    Log.Warning("Invalid HTTP Status Code");
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning("Failed fetching Imgur cat", ex.InnerException);
            }

            return null;
        }

        private static Response Deserialise(string content)
            => JsonConvert.DeserializeObject<Response>(content);

        private static string GetCachedCat()
        {
            // Get the top of the list
            var cachedCat = LinkPool.First();

            // Remove the URL, so we don't recycle it
            LinkPool.Remove(cachedCat);

            return cachedCat;
        }
    }
}