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
        private int ImgurPageNumber { get; set; } = 1;

        private readonly HttpClient _httpClient = new HttpClient();

        /// I want to change this link pool to be a list of objects
        /// I want to check these fields and make sure that when the user
        /// requests a gif, that the animated is set to true
        /// and it is of a image/gif type
        /// 
        /// Example response json
        /// "type": "image/jpeg",
        /// "animated": false,
        /// 
        /// Same goes for still pictures?
        /// animated should be false
        /// and type should be image/jpeg
        

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
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Fetching a cached cat");
                    catUrl = GetCachedCat(type);
                }
                else
                {
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
                        var links = imgur.Images.Select(x => x.Link).ToList();

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
            var cachedCat = string.Empty;

            if (type == CatMediaType.Gif)
            {
                if(LinkPool.Exists(x => x.EndsWith("gif")))
                {
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Pulling the first cat gif we can find");
                    cachedCat = LinkPool.First(x => x.EndsWith("gif"));
                }
                else
                {
                    return null;
                }
            }
            else if(type == CatMediaType.Jpg)
            {
                if(LinkPool.Exists(x => !x.EndsWith("gif")))
                {
                    Log.Information($"[{typeof(ImgurCatApi).Name}] Pulling the first cat picture we can find");
                    cachedCat = LinkPool.First(x => !x.EndsWith("gif"));
                }
                else
                {
                    return null;
                }
            }

            // Remove the URL, so we don't recycle it
            LinkPool.Remove(cachedCat);

            Log.Information($"[{typeof(ImgurCatApi).Name}] {LinkPool.Count} Cats in the pool");

            return cachedCat;
        }
    }
}