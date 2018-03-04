namespace Modix.Services.Cat
{
    using System;
    using System.Collections.Generic;
    using Serilog;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Services.Cat.Primary;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    enum ApiUrl
    {
        Primary,
        Secondary,
        Tertiary
    }

    public interface ICatService
    {
        //Task<string> HandleCat(Media mediaType, CancellationToken token);
        void Poke();
    }

    public class CatService : ICatService
    {
        private const string imgurClientId = "c482f6336b58ec4";
        private const string _primaryApi = "https://api.imgur.com/3/gallery/r/cats/page/";
        private const string _secondaryApi = "http://thecatapi.com/api/images/get?format=xml&results_per_page=10";
        private const string _tertiaryApi = "http://random.cat/meow";

        private static readonly Random Random = new Random();
        private static readonly HttpClient Client = new HttpClient();
        private static List<Primary.Datum> webpageCache;

        private int _imgurPageNumber { get; set; } = 1; 

        public CatService()
        {
            // Add the HTML header for authorization purpose for the imgur api
            Client.DefaultRequestHeaders.Add("Authorization", $"Client-ID {imgurClientId}");

            // Code to build web cache should go somewhere. AsyncLazy?
        }

        public string RandomizeCat()
        { 
            // Choose a random number
            var random = Random.Next(0, webpageCache.Count);

            // Select a random cat from the link and get it's link
            // Sometimes due to the Imgur API and how the images are uploaded, the links will be in different places within the JSON
            // I believe this is due to how albums work
            // If the webpageCache.link is blank, then the link to the image will be in webpageCache.images[].link
            // .images[] can have multiple images within it. I just select the first one here.
            var catUrl = webpageCache[random].link ?? webpageCache[random].images[1].link;
            
            // Remove the cat from the link
            webpageCache.RemoveAt(random);

            // Return the URL
            return catUrl;
        }

        /*public async Task<string> HandleCat(Media mediaType, CancellationToken token)
        {
            var catUrl = string.Empty;
            string json;

            while (!token.IsCancellationRequested)
            {
	            try
                {
	                //Download a json string from the API
	                //json = await DownloadCatJson(token);
	            }
                catch (TaskCanceledException)
                {
                    Log.Warning("Could not retrieve JSON string within the alloted time. Is the API Down?");
                    return "Cat not downloaded in time";
                }

                // Check and make sure the string isn't empty before attempting to deserialize the
                // json. If the website returns a blank string, something is wrong. Break the loop
                if (string.IsNullOrWhiteSpace(json))
                {
                    break;
                }

                catUrl = JObject.Parse(json)["file"].Value<string>();

                switch (mediaType)
                {
                    case Media.Picture when catUrl.EndsWith(".gif"):
                        continue;
                    case Media.Gif when !catUrl.EndsWith(".gif"):
                        continue;
                }

                break;
            }

            return catUrl;
        }*/

        // Method handles the logic for selecting the api.
        // Handles switch to fallback apis if necessary
        private async Task ApiLogicHandler()
        {

        }

        public void Poke()
        {
            // This is a debug command that serves to initialize the class because the class needs a first request to be initialized and ran
        }

        // Primary API will always be cache. Fallback APIs will be on demand.
        public async Task<List<Primary.Datum>> BuildWebCache()
        {
            var objsDatums = new List<Datum>();
            var primaryJson = string.Empty;

            // Download the json from the Imgur api
            using (var cts = new CancellationTokenSource(5000))
            {
                var token = cts.Token;
                primaryJson = await DownloadRequest($"{_primaryApi}{_imgurPageNumber}", token);
            }

            // Deserialize the json
            var primaryObject = DeserializeJson(primaryJson);

            // Add the deserialized json objects to the Webcache list we have
            objsDatums.AddRange(primaryObject.data);

            return objsDatums;
        }

        private Primary.Rootobject DeserializeJson(string content)
        {
            // This is the primary method for deserializing the json from the imgur api
            return JsonConvert.DeserializeObject<Primary.Rootobject>(content);
        }

        private Secondary.response DeserializeXML(string content)
        {
            // This is the secondary method for deserializing the xml from the thecatapi.com
            return content.DeserializeXML<Secondary.response>();
        }

        private string ParseJson(string content)
        {
            // This is the tertiary method for parsing the json from the random.cat/meow api
            return JObject.Parse(content)["file"].Value<string>();
        }

        private async Task<string> DownloadRequest(string apiUrl, CancellationToken token)
        {
            var content = string.Empty;

            try
            {
                using (var response = await Client.GetAsync(apiUrl,token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        // Download the content
                        content = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // If there is a bad result, empty string will be returned
                        Log.Warning("Invalid HTTP Status Code");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                Log.Warning("HTTP Request Exception", hre.InnerException);
            }

            return content;
        }
    }
}