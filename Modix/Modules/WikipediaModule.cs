using Discord;
using Discord.Commands;
using Modix.Services.StackExchange;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("Wikipedia"), Summary("Search Wikipedia from Discord!")]
    public class WikipediaModule : ModuleBase
    {
        [Command("wiki"), Summary("Returns a Wikipedia page result matching the search phrase.")]
        public async Task Run([Remainder] string phrase)
        {
            var response = await new WikipediaService().GetWikipediaResultsAsync(phrase);

            if (response != null && response.Query != null && response.Query.Pages.Any())
            {
                var messageBuilder = new StringBuilder();
                foreach (var page in response.Query.Pages.Values)
                {
                    if (string.IsNullOrWhiteSpace(page.Extract))
                    {
                        await ReplyAsync($"Failed to find anything for `!wiki {phrase}`.");
                    }
                    else
                    {
                        messageBuilder.AppendLine(page.Extract);
                    }
                }

                var message = messageBuilder.ToString();

                if (message.Length > 0)
                {
                    if (message.Length > DiscordConfig.MaxMessageSize)
                    {
                        // How many batches do we need to send?
                        // IE: 5000 / 2000 = 2.5
                        // Round up = 3
                        decimal batchCount = Math.Ceiling(decimal.Divide(message.Length, DiscordConfig.MaxMessageSize));
                        int cursor = 0;
                        for (var i = 0; i < batchCount; i++)
                        {
                            var builder = new EmbedBuilder()
                               .WithColor(new Color(95, 186, 125))
                               .WithTitle($"Results for {phrase} (pt {i + 1})")
                               .WithDescription(message.Substring(cursor, (i == batchCount - 1) ? message.Length - cursor : DiscordConfig.MaxMessageSize));
                            builder.Build();
                            await ReplyAsync("", embed: builder);
                            cursor += DiscordConfig.MaxMessageSize;
                        }
                    }
                    else
                    {
                        var builder = new EmbedBuilder()
                            .WithColor(new Color(95, 186, 125))
                            .WithTitle($"Results for {phrase}")
                            .WithDescription(message);
                        builder.Build();
                        await ReplyAsync("", embed: builder);
                    }
                }
            }
        }
    }
}
