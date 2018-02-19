namespace Modix.Modules
{
    using Discord.Commands;
    using Modix.Services.Cat;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;

    public class CatModule : ModuleBase
    {
        private static Media MediaType;
        private readonly ICatService _catService;

        public CatModule(ICatService catService)
        {
            _catService = catService;
        }

        public enum Media
        {
            Picture, // 0
            Gif // 1
        }

        [Command("cat"), Summary("Gets a cat")]
        public async Task Cat(string param = "")
        {
            var message = string.Empty;

            try
            {
                MediaType = (Media) Enum.Parse(typeof(Media), param, true);

                if (!Enum.IsDefined(typeof(Media), MediaType) && !MediaType.ToString().Contains(","))
                {
                    // Invalid parameter received
                    await Context.Channel.SendMessageAsync("Use `!cat` or `!cat gif`");
                    return;
                }
            }catch(Exception)
            {
                Log.Warning("Invalid Parameter Passed");
            }

            using (var cts = new CancellationTokenSource(5000))
            {
                var token = cts.Token;

                message = await _catService.HandleCat(MediaType, token);
            }

            // Send the link
            await Context.Channel.SendMessageAsync(message);
        }
    }
}