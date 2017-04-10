using Discord;
using Discord.Commands;
using Modix.Services.StackExchange;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("StackExchange"), Summary("Query any site from Stack Exchange.")]
    public class StackExchangeModule : ModuleBase
    {
        [Command("stack"), Summary("Returns top results from a Stack Exchange site. Usage: `!stack how do i parse json with c#? [site=stackoverflow tags=c#,json]`")]
        public async Task Run([Remainder] string phrase)
        {
            var pattern = new Regex(@"\[(site=\w*)?(.*)?(tags=\w*)?\]", RegexOptions.IgnoreCase);
            var matches = pattern.Matches(phrase);
            if(matches != null && matches.Count >= 0)
            {
                Match match = matches[0];

            }

            var response = await new StackExchangeService().GetStackExchangeResultsAsync(phrase);

            foreach (var res in response.Items.Take(3))
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(46, 204, 113))
                    .WithTitle($"{res.Score}: {WebUtility.HtmlDecode(res.Title)}")
                    .WithUrl(res.Link);
               
                builder.Build();
                await ReplyAsync("", embed: builder);
            }
        }
    }
}
