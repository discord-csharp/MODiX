using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
            string emojiUrl;

            if (Emote.TryParse(emoji, out var found))
            {
                emojiUrl = found.Url;
            }   
            else
            {
                var codepoint = char.ConvertToUtf32(emoji, 0);
                var codepointHex = codepoint.ToString("X").ToLower();

                emojiUrl = $"https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/{codepointHex}.png";
            }

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
