using System.IO;
using System.Net.Http;
using System.Text;
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
                var sb = new StringBuilder();
                sb.Append("https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/");

                for(int i = 0; i < emoji.Length; i++)
                {
                    var codepoint = char.ConvertToUtf32(emoji, i);
                    var codepointHex = codepoint.ToString("x");

                    //ConvertToUtf32() might have parsed an extra character
                    //Therefore we make sure to skip the next one
                    //We still need to increment through the string in a char by char manner though since
                    //some characters used in emojis sometimes only occupy one character (16 bit) in size.
                    if (codepoint > 0xFFFF)
                        i++;

                    sb.Append(codepointHex);

                    if (i+1 < emoji.Length)
                        sb.Append("-");
                }

                sb.Append(".png");
                emojiUrl = sb.ToString();
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
