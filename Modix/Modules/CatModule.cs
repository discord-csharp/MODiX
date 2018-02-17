namespace Modix.Modules
{
    using Discord.Commands;
    using Modix.Services.Cat;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class CatModule : ModuleBase
    {
        private static readonly HttpClient Client = new HttpClient();

        [Command("cat"), Summary("Gets a cat")]
        public async Task Cat(string gif = "")
        {
            // 5 seconds to find a cat
            var cts = new CancellationTokenSource(5000);
            var token = cts.Token;

            var message = string.Empty;

            var cat = new CatService();

            // Regular picture
            if (!string.Equals("gif", gif, StringComparison.OrdinalIgnoreCase))
            {
                message = await cat.GetCatPicture(token);
            }
            else if (gif.Equals("gif", StringComparison.OrdinalIgnoreCase))
            {
                // Gif of a cat
                message = await cat.GetCatGif(token);
            }
            else
            {
                // Invalid command received
                await Context.Channel.SendMessageAsync("Use `!cat` or `!cat gif`");
            }

            // Send the link
            await Context.Channel.SendMessageAsync(message);

            cts.Dispose();
        }
    }
}