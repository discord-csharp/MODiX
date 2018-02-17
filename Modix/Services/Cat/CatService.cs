namespace Modix.Services.Cat
{
    using Newtonsoft.Json;
    using Serilog;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class CatService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Url for cat
        public struct URL
        {
            public string file { get; set; }
        }

        /// <summary>
        /// Gets the cat GIF.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Return the URL to the cat</returns>
        public async Task<string> GetCatGif(CancellationToken token)
        {
            var obj = new URL();
            var json = string.Empty;
            var fileFoundFlag = false;

            do
            {
                try
                {
                    //Download a json string from the API
                    json = await DownloadCatJson(token);
                }
                catch (TaskCanceledException)
                {
                    Log.Warning("Could not retrieve JSON string within the alloted time. Is the API Down?");
                    return "Cat not downloaded in time";
                }

                // Check and make sure the string isn't empty before attempting to deserialize the
                // json. If the website returns a blank string, something is wrong. Break the loop
                if (string.IsNullOrWhiteSpace(json)) break;

                //Deserialize the json retrieved from the website
                obj = DeserializeJson(json);

                // If the url doesn't end with .gif, skip to the while evaluation and start the loop again
                if (!obj.file.EndsWith(".gif")) continue;

                // If a file is found on the first try, set the flag to true and break out of the
                // loop. If the loop is canceled due to the CancellationToken, the flag remains false
                // and the 404 error message is returned.
                fileFoundFlag = true;
                break;
            } while (!token.IsCancellationRequested);

            //Return the URL to the picture or the error message
            return fileFoundFlag ? obj.file : "404 cat gif not found";
        }

        /// <summary>
        /// Gets the cat picture.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Returns the URL to the cat</returns>
        public async Task<string> GetCatPicture(CancellationToken token)
        {
            var json = string.Empty;
            var obj = new URL();
            var fileFoundFlag = false;

            do
            {
                try
                {
                    // Download a JSON string from the API
                    json = await DownloadCatJson(token);
                }
                catch (TaskCanceledException)
                {
                    Log.Warning("Could not retrieve JSON string within the alloted time. Is the API Down?");
                    return "Cat not downloaded in time";
                }

                // Check and make sure the string isn't empty before attempting to deserialize the
                // json. If the website returns a blank string, something is wrong. Break the loop
                if (string.IsNullOrWhiteSpace(json)) break;

                //Deserialize the json retrieved from the website
                obj = DeserializeJson(json);

                // We want a cat picture. If the URL ends with .gif, skip to the while evaluation and
                // start the loop again
                if (obj.file.EndsWith(".gif")) continue;

                // If a file is found on the first try, set the flag to true and break out of the
                // loop. If the loop is canceled due to the CancellationToken, the flag remains false
                // and the 404 error message is returned.
                fileFoundFlag = true;
                break;
            } while (!token.IsCancellationRequested);

            //Return the URL to the picture or the error message
            return fileFoundFlag ? obj.file : "404 cat picture not found";
        }

        /// <summary>
        /// Downloads the cat json.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns>Returns a JSON string</returns>
        private static async Task<string> DownloadCatJson(CancellationToken token)
        {
            var json = string.Empty;

            try
            {
                using (var response = await Client.GetAsync("http://random.cat/meow", token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the JSON from the API
                        json = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // If there is a bad result, an empty json string will be returned
                        Log.Warning("Invalid HTTP Status Code");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                Log.Error("Request Exception", hre.InnerException);
            }

            return json;
        }

        /// <summary>
        /// Deserializes the json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Returns a URL object</returns>
        private static URL DeserializeJson(string json)
        {
            // Deserialize JSON
            return JsonConvert.DeserializeObject<URL>(json);
        }
    }
}