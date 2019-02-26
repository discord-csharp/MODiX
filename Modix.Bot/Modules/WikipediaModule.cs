using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modix.Services.Wikipedia;

namespace Modix.Modules
{
    [Name("Wikipedia"), Summary("Search Wikipedia from Discord!")]
    public class WikipediaModule : ModuleBase
    {
        public WikipediaModule(WikipediaService wikipediaService)
        {
            WikipediaService = wikipediaService;
        }

        [Command("wiki"), Summary("Returns a Wikipedia page result matching the search phrase.")]
        public async Task Run([Remainder] string phrase)
        {
            var response = await WikipediaService.GetWikipediaResultsAsync(phrase);

            // Empty response.
            if (response == null || response.Query == null || !response.Query.Pages.Any())
            {
                await ReplyAsync($"Failed to find anything for `!wiki {phrase}`.");
                return;
            }

            // Construct results into one string (use StringBuilder for concat speed).
            var messageBuilder = new StringBuilder();
            response.Query.Pages.Values.ToList().ForEach(p => messageBuilder.AppendLine(p.Extract));
            var message = messageBuilder.ToString();

            // Sometimes we get here and there's no message, just double check.
            if (message.Length == 0 || message == Environment.NewLine)
            {
                await ReplyAsync($"Failed to find anything for `!wiki {phrase}`.");
                return;
            }

            // Discord has a limit on channel message size... so this accounts for that.
            if (message.Length > DiscordConfig.MaxMessageSize)
            {
                // How many batches do we need to send?
                // IE: 5000 / 2000 = 2.5
                // Round up = 3
                var batchCount = Math.Ceiling(decimal.Divide(message.Length, DiscordConfig.MaxMessageSize));

                // Keep track of how many characters we've sent to the channel.
                // Use the cursor to see the diff between what we've sent, and what is remaining to send
                //  So we can satisfy the batch sending approach.
                var cursor = 0;

                for (var i = 0; i < batchCount; i++)
                {
                    var builder = new EmbedBuilder()
                       .WithColor(new Color(95, 186, 125))
                       .WithTitle($"Results for {phrase} (pt {i + 1})")
                       .WithDescription(message.Substring(cursor, (i == batchCount - 1) ? message.Length - cursor : DiscordConfig.MaxMessageSize));
                    
                    await ReplyAsync("", embed: builder.Build());
                    cursor += DiscordConfig.MaxMessageSize;
                }
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithTitle($"Results for {phrase}")
                    .WithDescription(message);

                await ReplyAsync("", embed: builder.Build());
            }
        }

        protected WikipediaService WikipediaService { get; }
    }
}
