namespace Modix.Services.Cat.APIs.Imgur
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Serilog;

    public class ImgurCatApi : ICatApi
    {
        // This can be public
        private const string ClientId = "c482f6336b58ec4";

        // TODO Add rollover logic for multiple pages. 
        private const string Url = "https://api.imgur.com/3/gallery/r/cats/page/";
        private byte ImgurPageNumber { get; set; }
        private byte ImgurPageMaximum { get; } = 5;

        private readonly HttpClient _httpClient = new HttpClient();
        private static readonly List<Image> LinkPool = new List<Image>();

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
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Fetching a cached cat");
                    catUrl = GetCachedCat(type);
                }
                else
                {
                    // This section executes if the pool is empty;

                    // Each time the pool empties we want to go to the next page
                    // Add one to the page
                    ImgurPageNumber += 1;

                    // Dirty page rollover code
                    if (ImgurPageNumber > ImgurPageMaximum)
                    {
                        // Reset back at page one
                        ImgurPageNumber = 1;
                    }

                    Log.Information($"[{typeof(ImgurCatApi).Name}] Attempting to retrieve cats from api");
                    var success = await BuildLinkCache(cancellationToken);

                    if (success)
                    {
                        catUrl = GetCachedCat(type);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning($"[{typeof(ImgurCatApi).Name}] Failed fetching Imgur cat", ex.InnerException);
            }

            if (!string.IsNullOrWhiteSpace(catUrl))
            {
                Log.Information($"[{typeof(ImgurCatApi).Name}] Successful cat retrieval");
                return new UrlCatResponse(catUrl);
            }
                
            Log.Information($"[{typeof(ImgurCatApi).Name}] Failed cat retrieval");
            return new UrlCatResponse();
        }

        private async Task<bool> BuildLinkCache(CancellationToken cancellationToken)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(Url, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"[{typeof(ImgurCatApi).Name}] HTTP Status Code - {response.StatusCode}");

                        Log.Information($"[{typeof(ImgurCatApi).Name}] Downloaded content");
                        var content = await response.Content.ReadAsStringAsync();

                        Log.Information($"[{typeof(ImgurCatApi).Name}] Deserializing content");
                        var imgur = Deserialise(content);

                        // We may have succeeded in the response, but Imgur may not like us,
                        // check if Imgur has returned us a successful result
                        if (!imgur.Success)
                        {
                            Log.Warning($"[{typeof(ImgurCatApi).Name}] Failed response from Imgur", imgur.Images[1].Error);
                            return false;
                        }

                        // We have a caching mechanism in place, therefore attempt to get
                        // all links of the URLs and cache
                        var links = imgur.Images.ToList();

                        Log.Information($"[{typeof(ImgurCatApi).Name}] Filling link pool");
                        LinkPool.AddRange(links);
                    }
                    else
                    {
                        // If there is a bad result, empty string will be returned
                        Log.Warning($"[{typeof(ImgurCatApi).Name}] Invalid HTTP Status Code", response.StatusCode);
                        return false;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning($"[{typeof(ImgurCatApi).Name}] Failed fetching Imgur cat", ex.InnerException);
                return false;
            }

            return true;
        }

        private static Response Deserialise(string content)
            => JsonConvert.DeserializeObject<Response>(content);

        private static string GetCachedCat(CatMediaType type)
        {
            var cachedCat = new Image();
            var cachedCatLink = string.Empty;

            switch (type)
            {
                case CatMediaType.Gif:
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Pulling the first cat gif we can find");
                    cachedCat = LinkPool.FirstOrDefault(x => x.Animated);
                    break;
                case CatMediaType.Jpg:
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Pulling the first cat picture we can find");
                    cachedCat = LinkPool.FirstOrDefault(x => !x.Animated);
                    break;
            }

            cachedCatLink = cachedCat?.Link;

            // Remove the cat object, so we don't recycle it
            LinkPool.Remove(cachedCat);

            Log.Information($"[{typeof(ImgurCatApi).Name}] {LinkPool.Count} Cats in the pool");

            return cachedCatLink;
        }
    }
}