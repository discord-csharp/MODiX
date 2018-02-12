namespace Modix.Modules
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Modix.Services.Cat;

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
            if (string.IsNullOrWhiteSpace(gif))
            {
                message = await cat.GetCatPicture(token);
            }
            else if (gif.Equals("gif"))
            {
                // Gif cat
                message = await cat.GetCatGif(token);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Use `!cat` or `!cat gif`");
            }

            await Context.Channel.SendMessageAsync(message);
        }
    }
}