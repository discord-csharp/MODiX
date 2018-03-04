using Discord.Commands;
using System;
using System.Threading;
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

        [Command(RunMode = RunMode.Async)]
        public async Task Cat(string param)
        {
            var type = !string.IsNullOrWhiteSpace(param) && param.Contains(Gif) ? CatMediaType.Gif : CatMediaType.Image;

            try
            {
                var catUrl = await _catService.Get(type);

                if (!string.IsNullOrWhiteSpace(catUrl))
                {
                    await ReplyAsync(catUrl);
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
    }
}