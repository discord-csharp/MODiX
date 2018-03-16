namespace Modix.Services.Animals.Cat.APIs.Imgur
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Modules;
    using Modix.Services.Animals;
    using Modix.Services.Cat.APIs.Imgur;
    using Newtonsoft.Json;
    using Serilog;
    using Response = Modix.Services.Cat.APIs.Imgur.Response;

    public class ImgurCatApi : IAnimalApi
    {
        // This can be public
        private const string ClientId = "c482f6336b58ec4";

        // TODO Add better rollover logic for multiple pages. 
        private const string Url = "https://api.imgur.com/3/gallery/r/cats/page/";
        private byte ImgurPageNumber { get; set; }
        private byte ImgurPageMaximum { get; } = 5;

        private readonly HttpClient _httpClient = new HttpClient();
        private static readonly List<Image> LinkPool = new List<Image>();

        public ImgurCatApi()
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {ClientId}");
        }

        public async Task<Animals.Response> Fetch(MediaType mediaType, CancellationToken cancellationToken = default)
        {
            string catUrl = null;

            try
            {
                // If we have any cat URLs in the pool, try to fetch those first
                if (LinkPool.Any())
                {
                    Log.Information($"[{nameof(ImgurCatApi)}] Fetching a cached cat");
                    catUrl = GetCachedCat(mediaType);
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

                    Log.Information($"[{nameof(ImgurCatApi)}] Attempting to retrieve cats from api");
                    var success = await BuildLinkCache(cancellationToken);

                    if (success)
                    {
                        catUrl = GetCachedCat(mediaType);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning($"[{nameof(ImgurCatApi)}] Failed fetching Imgur cat", ex.InnerException);
            }

            if (!string.IsNullOrWhiteSpace(catUrl))
            {
                Log.Information($"[{nameof(ImgurCatApi)}] Successful cat retrieval");
                return new UrlResponse(catUrl);
            }
                
            Log.Information($"[{nameof(ImgurCatApi)}] Failed cat retrieval");
            return new UrlResponse();
        }

        private async Task<bool> BuildLinkCache(CancellationToken cancellationToken)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(Url, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"[{nameof(ImgurCatApi)}] HTTP Status Code - {response.StatusCode}");

                        Log.Information($"[{nameof(ImgurCatApi)}] Downloaded content");
                        var content = await response.Content.ReadAsStringAsync();

                        Log.Information($"[{nameof(ImgurCatApi)}] Deserializing content");
                        var imgur = Deserialise(content);

                        // We may have succeeded in the response, but Imgur may not like us,
                        // check if Imgur has returned us a successful result
                        if (!imgur.Success)
                        {
                            Log.Warning($"[{nameof(ImgurCatApi)}] Failed response from Imgur", imgur.Images[1].Error);
                            return false;
                        }

                        // We have a caching mechanism in place, therefore attempt to get
                        // all links of the URLs and cache
                        var links = imgur.Images.ToList();

                        Log.Information($"[{nameof(ImgurCatApi)}] Filling link pool");
                        LinkPool.AddRange(links);
                    }
                    else
                    {
                        // If there is a bad result, empty string will be returned
                        Log.Warning($"[{nameof(ImgurCatApi)}] Invalid HTTP Status Code", response.StatusCode);
                        return false;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Warning($"[{nameof(ImgurCatApi)}] Failed fetching Imgur cat", ex.InnerException);
                return false;
            }

            return true;
        }

        private static Response Deserialise(string content)
            => JsonConvert.DeserializeObject<Response>(content);

        private static string GetCachedCat(MediaType type)
        {
            var cachedCat = new Image();
            var cachedCatLink = string.Empty;

            switch (type)
            {
                case MediaType.Gif:
                    Log.Information($"[{nameof(ImgurCatApi)}] Pulling the first cat gif we can find");
                    cachedCat = LinkPool.FirstOrDefault(x => x.Animated);
                    break;
                case MediaType.Jpg:
                    Log.Information($"[{nameof(ImgurCatApi)}] Pulling the first cat picture we can find");
                    cachedCat = LinkPool.FirstOrDefault(x => !x.Animated);
                    break;
            }

            cachedCatLink = cachedCat?.Link;

            // Remove the cat object, so we don't recycle it
            LinkPool.Remove(cachedCat);

            Log.Information($"[{nameof(ImgurCatApi)}] {LinkPool.Count} Cats in the pool");

            return cachedCatLink;
        }
    }
}