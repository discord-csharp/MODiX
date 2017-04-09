using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Csharp;

namespace Modix.Modules
{
    [Name("Documentation"), Summary("Easy way to ban the bad guys.")]
    public class DocumentationModule : ModuleBase
    {
        [Command("docs"), Summary("SShows class/method reference from the new unified .Net reference.")]
        public async Task GetDocumentationAsync(string term)
        {
            var results = await new DocumentationService().GetDocumentationResultsAsync(term);

            var builder = new EmbedBuilder();
            builder.WithColor(new Color(46, 204, 113))
                .WithTitle($"Documentation for '{term}'");

            var embedCount = 0; //boy, fuck d.net and their "hide everything" shit

            foreach (var res in results.Results)
            {
                if (embedCount == 5) break;
                embedCount++;

                builder.AddField(
                    new EmbedFieldBuilder()
                        .WithName($"[{res.DisplayName}]({res.Url})")
                        .WithValue("...")
                );
            }
            builder.Build();
            await ReplyAsync("", embed: builder);
        }
    }
}
