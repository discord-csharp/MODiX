namespace Modix.Modules
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Newtonsoft.Json;
    using Serilog;

    public class CatModule : ModuleBase
    {
        // Url for cat
        public struct URL
        {
            public string file { get; set; }
        }

        private static readonly HttpClient Client = new HttpClient();

        [Command("cat"), Summary("Gets a cat")]
        public async Task Cat(string gif = "")
        {
            // 30 seconds to find a cat
            var token = new CancellationTokenSource(30000);

            // Regular picture
            if (gif.Equals(string.Empty))
            {
                await GetCatPicture(token);
            }
            else if (gif.Equals("gif"))
            {
                // Gif cat
                await GetCatGif(token);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Use `!cat` or `!cat gif`");
            }
        }

        private async Task GetCatGif(CancellationTokenSource token)
        {
            var obj = new URL();
            var json = string.Empty;
            var flag = false;

            // five chances to find a cat gif
            for (var i = 0; i < 5; i++)
            {
                json = await DownloadCatJson(token);

                // Check and make sure the string isn't empty before attempting to deserialize cat
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Error("Empty JSON String");
                    return;
                }

                obj = DeserializeJson(json);

                if (obj.file.EndsWith(".gif"))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                await Context.Channel.SendMessageAsync(obj.file);
            }
            else
            {
                await Context.Channel.SendMessageAsync("404 cat gif not found");
            }
        }

        private async Task GetCatPicture(CancellationTokenSource token)
        {
            var json = string.Empty;
            var obj = new URL();
            var flag = false;

            // 5 chances to find a cat picture
            for (var i = 0; i < 5; i++)
            {
                json = await DownloadCatJson(token);

                // Check and make sure the string isn't empty before attempting to deserialize cat
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Error("Empty JSON String");
                    return;
                }

                obj = DeserializeJson(json);

                if (!obj.file.EndsWith(".gif"))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                await DownloadCatPicture(obj.file, token);
            }
            else
            {
                await Context.Channel.SendMessageAsync("404 cat picture not found");
            }
        }

        private async Task<string> DownloadCatJson(CancellationTokenSource token)
        {
            var json = string.Empty;

            try
            {
                using (var response = await Client.GetAsync("http://random.cat/meow"))
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
                Log.Fatal($"Request Exception", hre.InnerException);
            }
            catch (TaskCanceledException)
            {
                await Context.Channel.SendMessageAsync("Could not find cat in time");
            }

            return json;
        }

        private async Task DownloadCatPicture(string url, CancellationTokenSource token)
        {
            var srcToken = token.Token;

            try
            {
                using (var response = await Client.GetAsync(url, srcToken))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            await Context.Channel.SendFileAsync(stream, "cat.jpg");
                        }
                    }
                    else
                    {
                        Log.Error("Invalid HTTP Status Code");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                await Context.Channel.SendMessageAsync("Count not download cat in time");
            }
        }

        private static URL DeserializeJson(string json)
        {
            return JsonConvert.DeserializeObject<URL>(json);
        }
    }
}