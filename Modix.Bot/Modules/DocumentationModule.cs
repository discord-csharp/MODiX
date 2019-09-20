using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System;
using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;
using Modix.Services.Csharp;

namespace Modix.Modules
{
    [Name("Documentation")]
    [Summary("Search for information within the .NET docs.")]
    [HelpTags("docs")]
    public class DocumentationModule : ModuleBase
    {
        public DocumentationModule(DocumentationService documentationService)
        {
            DocumentationService = documentationService;
        }

        [Command("docs"), Summary("Shows class/method reference from the new unified .NET reference.")]
        [Alias("explain")]
        public async Task GetDocumentationAsync(
            [Remainder]
            [Summary("The term to search for in the documentation.")]
                string term)
        {
            Regex reg = new Regex("^[0-9A-Za-z.<>]$");
            foreach(char c in term)
            {
                if(!reg.IsMatch(c.ToString()))
                {
                    await ReplyAsync($" '{c}' character is not allowed in the search, please try again.");
                    return;
                }
            }

            var response = await DocumentationService.GetDocumentationResultsAsync(term);

            if (response.Count == 0)
            {
                await ReplyAsync("Could not find documentation for your requested term.");
                return;
            }

            var embedCount = 0;

            var stringBuild = new StringBuilder();

            foreach (var res in response.Results.Take(3))
            {
                embedCount++;
                stringBuild.AppendLine($"**\u276F [{res.ItemKind}: {res.DisplayName}]({res.Url})**");
                stringBuild.AppendLine($"{res.Description}");
                stringBuild.AppendLine();

                if (embedCount == 3)
                {
                    stringBuild.Append(
                        $"{embedCount}/{response.Results.Count} results shown ~ [Click Here for more results](https://docs.microsoft.com/dotnet/api/?term={term})"
                    );
                }
            }
            var buildEmbed = new EmbedBuilder()
            {
                Description = stringBuild.ToString()
            }.WithColor(new Color(46, 204, 113));

            await ReplyAsync(embed: buildEmbed.Build());
        }

        protected DocumentationService DocumentationService { get; }
    }
}
