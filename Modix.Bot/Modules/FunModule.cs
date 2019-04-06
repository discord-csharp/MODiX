using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Modules
{
    [Name("Fun"), Summary("A bunch of miscellaneous, fun commands.")]
    public class FunModule : ModuleBase
    {
        public FunModule(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [Command("jumbo"), Summary("Jumbofy an emoji.")]
        public async Task JumboAsync(
            [Summary("The emoji to jumbofy.")]
                string emoji)
        {
            var emojiUrl = EmojiUtilities.GetUrl(emoji);

            try
            {
                var client = HttpClientFactory.CreateClient();
                var req = await client.GetStreamAsync(emojiUrl);

                await Context.Channel.SendFileAsync(req, Path.GetFileName(emojiUrl), Context.User.Mention);
                
                try
                {
                    await Context.Message.DeleteAsync();
                }
                catch (HttpRequestException)
                {
                    Log.Information("Couldn't delete message after jumbofying.");
                }
            }
            catch (HttpRequestException)
            {
                await ReplyAsync($"Sorry {Context.User.Mention}, I don't recognize that emoji.");
            }
        }

        protected IHttpClientFactory HttpClientFactory { get; }
    }
}
