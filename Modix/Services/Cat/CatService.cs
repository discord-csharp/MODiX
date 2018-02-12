namespace Modix.Services.Cat
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Serilog;

    public class CatService
    {
        private static readonly HttpClient Client = new HttpClient();

        // Url for cat
        public struct URL
        {
            public string file { get; set; }
        }

        public async Task<string> GetCatGif(CancellationToken token)
        {
            var obj = new URL();
            var json = string.Empty;
            var flag = false;

            do
            {
                json = await DownloadCatJson(token);

                // Check and make sure the string isn't empty before attempting to deserialize cat
                if (string.IsNullOrWhiteSpace(json)) break;

                obj = DeserializeJson(json);

                if (!obj.file.EndsWith(".gif")) continue;

                flag = true;
                break;

            } while (!token.IsCancellationRequested);

            return flag ? obj.file : "404 cat gif not found";
        }

        public async Task<string> GetCatPicture(CancellationToken token)
        {
            var json = string.Empty;
            var obj = new URL();
            var flag = false;

            do
            {
                json = await DownloadCatJson(token);

                // Check and make sure the string isn't empty before attempting to deserialize cat
                if (string.IsNullOrWhiteSpace(json)) break;

                obj = DeserializeJson(json);

                if (obj.file.EndsWith(".gif")) continue;

                flag = true;
                break;

            } while (!token.IsCancellationRequested);

            return flag ? obj.file : "404 cat picture not found";
        }

        private async Task<string> DownloadCatJson(CancellationToken token)
        {
            var json = string.Empty;

            try
            {
                using (var response = await Client.GetAsync("http://random.cat/meow", token))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        json = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // If there is a bad result, an empty json string will be returned
                        Log.Error("Invalid HTTP Status Code");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                Log.Fatal("Request Exception", hre.InnerException);
            }
            catch (TaskCanceledException)
            {
                return "Could not find cat in time";
            }

            return json;
        }

        private static URL DeserializeJson(string json)
        {
            return JsonConvert.DeserializeObject<URL>(json);
        }
    }
}