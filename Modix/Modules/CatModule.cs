using Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;
using Modix.Services.Cat;
using Serilog;

namespace Modix.Modules
{
    [Summary("Cat Related Commands")]
    public class CatModule : ModuleBase
    {
        private readonly ICatService _catService;

        private const string Gif = "gif";

        public CatModule(ICatService catService)
        {
            _catService = catService;
        }

        [Command("cat", RunMode = RunMode.Async)]
        public async Task Cat(string parameter = null)
        {
            var type = !string.IsNullOrWhiteSpace(parameter) && parameter.Contains(Gif) ? CatMediaType.Gif : CatMediaType.Jpg;

            try
            {
                var cat = await _catService.Get(type);

                if (cat != null)
                {
                    await ProcessCatResponse(type, cat);
                }
                else
                {
                    await ReplyAsync("The cat vending machine has run out :(");
                }
            }
            catch (TaskCanceledException)
            {
                await ReplyAsync("Couldn't get a cat in time :(");
            }
            catch (Exception ex)
            {
                Log.Error("Failed getting cat", ex);
            }
        }

        private async Task ProcessCatResponse(CatMediaType type, CatResponse cat)
        {
            switch (cat)
            {
                case ByteCatResponse byteResponse:
                    var fileName = "cat." + type.ToString().ToLower();

                    using (var stream = new MemoryStream(byteResponse.Bytes))
                        await Context.Channel.SendFileAsync(stream, fileName);
                    break;

                case UrlCatResponse urlResponse:
                    await ReplyAsync(urlResponse.Url);
                    break;

                default:
                    await ReplyAsync("Something went wrong while finding a kitty");
                    break;
            }
        }
    }
}