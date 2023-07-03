#nullable enable

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.StackExchange;

namespace Modix.Modules
{
    [ModuleHelp("Stack Exchange", "Query any site from Stack Exchange.")]
    public class StackExchangeModule : InteractionModuleBase
    {
        private readonly StackExchangeService _stackExchangeService;
        private readonly string? _stackOverflowToken;

        public StackExchangeModule(
            IOptions<ModixConfig> config,
            StackExchangeService stackExchangeService)
        {
            _stackExchangeService = stackExchangeService;
            _stackOverflowToken = config.Value.StackoverflowToken;
        }

        [SlashCommand("stackexchange", "Returns top results from a Stack Exchange site.")]
        public async Task RunAsync(
            [Summary(description: "The phrase to search Stack Exchange for.")]
                string phrase)
        {
            var startLocation = phrase.IndexOf("[");
            var endLocation = phrase.IndexOf("]");

            string? site = null;
            string? tags = null;

            if (startLocation > 0 && endLocation > 0)
            {
                var query = phrase.Substring(startLocation, endLocation - (startLocation - 1));
                var parts = query.Replace("[", "").Replace("]", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    if (part.Contains("site=", StringComparison.OrdinalIgnoreCase))
                    {
                        site = part.Split(new[] { "site=" }, StringSplitOptions.None)[1];
                    }
                    else if (part.Contains("tags=", StringComparison.OrdinalIgnoreCase))
                    {
                        tags = part.Split(new[] { "tags=" }, StringSplitOptions.None)[1];
                    }
                }

                phrase = phrase.Remove(startLocation, endLocation - (startLocation - 1)).Trim();
            }

            site ??= "stackoverflow";
            tags ??= "c#";

            var response = await _stackExchangeService.GetStackExchangeResultsAsync(_stackOverflowToken, phrase, site, tags);
            var filteredRes = response.Items.Where(x => x.Tags.Contains(tags));

            var firstResponse = true;

            foreach (var res in filteredRes.Take(3))
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithTitle($"{res.Score}: {WebUtility.HtmlDecode(res.Title)}")
                    .WithUrl(res.Link);

                if (firstResponse)
                {
                    await FollowupAsync(embed: builder.Build());
                    firstResponse = false;
                }
                else
                {
                    await ReplyAsync(embed: builder.Build());
                }
            }

            var footer = new EmbedBuilder()
                .WithColor(new Color(50, 50, 50))
                .WithFooter(
                     new EmbedFooterBuilder().WithText($"tags: {tags} | site: {site}. !stack foobar [site=stackexchange tags=c#]"));

            await ReplyAsync(embed: footer.Build());
        }
    }
}
