using Discord;
using Discord.Commands;
using Modix.Services.StackExchange;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("StackExchange"), Summary("Query any site from Stack Exchange.")]
    public class StackExchangeModule : ModuleBase
    {
        [Command("stack"), Summary("Returns top results from a Stack Exchange site. Usage: `!stack how do i parse json with c#? [site=stackoverflow tags=c#,json]`")]
        public async Task Run([Remainder] string phrase)
        {
            int startLocation = phrase.IndexOf("[");
            int endLocation = phrase.IndexOf("]");

            string site = null;
            string tags = null;

            if (startLocation > 0 && endLocation > 0)
            {
                var query = phrase.Substring(startLocation, endLocation - (startLocation - 1));
                var parts = query.Replace("[", "").Replace("]", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    if (part.IndexOf("site=") >= 0)
                    {
                        site = part.Split(new[] { "site=" }, StringSplitOptions.None)[1];
                    }
                    else if (part.IndexOf("tags=") >= 0)
                    {
                        tags = part.Split(new[] { "tags=" }, StringSplitOptions.None)[1];
                    }
                }

                phrase = phrase.Remove(startLocation, endLocation - (startLocation - 1)).Trim();
            }

            if (site == null)
            {
                site = "stackoverflow";
            }

            if (tags == null)
            {
                tags = "c#";
            }

            var response = await new StackExchangeService().GetStackExchangeResultsAsync(phrase, site, tags);

            foreach (var res in response.Items.Take(3))
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithTitle($"{res.Score}: {WebUtility.HtmlDecode(res.Title)}")
                    .WithUrl(res.Link);

                builder.Build();
                await ReplyAsync("", embed: builder);
            }

            var footer = new EmbedBuilder()
                .WithColor(new Color(50, 50, 50))
                .WithFooter(
                     new EmbedFooterBuilder().WithText($"tags: {tags} | site: {site}. !stack foobar [site=stackexchange tags=c#]"));
            footer.Build();
            await ReplyAsync("", embed: footer);
        }
    }
}
