using Discord;
using Discord.Commands;
using Modix.Services.StackExchange;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Microsoft.Extensions.Options;

namespace Modix.Modules
{
    [Name("Stack Exchange"), Summary("Query any site from Stack Exchange.")]
    public class StackExchangeModule : ModuleBase
    {
        private readonly string _stackOverflowToken;

        public StackExchangeModule(
            IOptions<ModixConfig> config,
            StackExchangeService stackExchangeService)
        {
            StackExchangeService = stackExchangeService;
            _stackOverflowToken = config.Value.StackoverflowToken;
        }

        [Command("stack"), Summary("Returns top results from a Stack Exchange site."), Remarks("Usage: `!stack how do i parse json with c#? [site=stackoverflow tags=c#,json]`")]
        public async Task RunAsync(
            [Remainder]
            [Summary("The phrase to search Stack Exchange for.")]
                string phrase)
        {
            var startLocation = phrase.IndexOf("[");
            var endLocation = phrase.IndexOf("]");

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

            var response = await StackExchangeService.GetStackExchangeResultsAsync(_stackOverflowToken, phrase, site, tags);
            var filteredRes = response.Items.Where(x => x.Tags.Contains(tags));
            foreach (var res in filteredRes.Take(3))
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithTitle($"{res.Score}: {WebUtility.HtmlDecode(res.Title)}")
                    .WithUrl(res.Link);

                await ReplyAsync("", embed: builder.Build());
            }

            var footer = new EmbedBuilder()
                .WithColor(new Color(50, 50, 50))
                .WithFooter(
                     new EmbedFooterBuilder().WithText($"tags: {tags} | site: {site}. !stack foobar [site=stackexchange tags=c#]"));

            await ReplyAsync("", embed: footer.Build());
        }

        protected StackExchangeService StackExchangeService { get; }
    }
}
