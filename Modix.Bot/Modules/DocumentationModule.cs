using System.Linq;
using System.Threading.Tasks;
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
        public async Task GetDocumentationAsync(
            [Remainder]
            [Summary("The term to search for in the documentation.")]
                string term)
        {
            var response = await DocumentationService.GetDocumentationResultsAsync(term);

            if (response.Count == 0)
            {
                await ReplyAsync("Could not find documentation for your requested term.");
                return;
            }
            
            var embedCount = 0;

            foreach (var res in response.Results.Take(3).OrderBy(x => x.DisplayName))
            {
                embedCount++;

                var builder = new EmbedBuilder()
                    .WithColor(new Color(46, 204, 113))
                    .WithTitle($"{res.ItemKind}: {res.DisplayName}")
                    .WithUrl(res.Url)
                    .WithDescription(res.Description);

                if (embedCount == 3)
                {
                    builder.WithFooter(
                        new EmbedFooterBuilder().WithText($"{embedCount}/{response.Results.Count} https://docs.microsoft.com/dotnet/api/?term={term}")
                    );
                    builder.Footer.Build();
                }

                await ReplyAsync(embed: builder.Build());
            }
        }

        protected DocumentationService DocumentationService { get; }
    }
}
