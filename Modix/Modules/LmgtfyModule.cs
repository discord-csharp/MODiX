using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix.Modules
{
    [Name("Let Me Google That For You")]
    [Summary("Sometimes we all need that little extra push")]
    public sealed class LmgtfyModule : ModuleBase
    {
        private static readonly string[] WordChoices =
        {
            "your highness",
            "your majesty",
            "your excellency",
            "my liege",
            "o' great one",
            "shatterer of a thousand worlds",
            "dark lord",
            "prince of thunder",
            "great bambino",
            "sultan of swat",
            "titan of terror",
            "colossus of clout"
        };

        private static readonly Random Random = new Random();

        [Command("lmgtfy")]
        [Summary("Creates a lmgtfy.com link")]
        public async Task LmgtfyAsync([Remainder] string query)
        {
            // Remove extra white space
            var singleSpaceRegex = new Regex("[ ]{2,}");
            query = singleSpaceRegex.Replace(query, " ");
            query = WebUtility.UrlEncode(string.Join(" ", query));

            var url = $"http://lmgtfy.com/?q={query}";

            var builder = new EmbedBuilder()
                .WithTitle($"Let me google that for you, {WordChoices[Random.Next(0, WordChoices.Length)]}")
                .WithColor(new Color(95, 186, 125))
                .WithUrl(url);
            await ReplyAsync(string.Empty, embed: builder);
        }
    }
}