namespace Modix.Services.Cat
{
    using Newtonsoft.Json;
    using Serilog;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Modix.Modules;

    public interface ICatService
    {
        Task<string> HandleCat(CatModule.Media mediaType, CancellationToken token);
    }

    public class CatService : ICatService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Url for cat
        private struct URL
        {
            public string file { get; set; }
        }

        public async Task<string> HandleCat(CatModule.Media mediaType, CancellationToken token)
        {
            var obj = new URL();
            var json = string.Empty;
            var fileFoundFlag = false;

            while (!token.IsCancellationRequested)
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

                switch (mediaType)
                {
                    case CatModule.Media.Picture when obj.file.EndsWith(".gif"):
                        continue;
                    case CatModule.Media.Gif when !obj.file.EndsWith(".gif"):
                        continue;
                }

                // If a file is found on the first try, set the flag to true and break out of the
                // loop. If the loop is canceled due to the CancellationToken, the flag remains false
                // and the 404 error message is returned.
                fileFoundFlag = true;
                break;
            }

            return fileFoundFlag ? obj.file : "404 cat gif not found";
        }

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

        private static URL DeserializeJson(string json)
        {
            // Deserialize JSON
            return JsonConvert.DeserializeObject<URL>(json);
        }
    }
}