using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Csharp;

namespace Modix.Modules
{
    [Name("Documentation"), Summary("Easy way to ban the bad guys.")]
    public class DocumentationModule : ModuleBase
    {
        [Command("docs"), Summary("Shows class/method reference from the new unified .Net reference.")]
        public async Task GetDocumentationAsync([Remainder]string term)
        {
            var response = await new DocumentationService().GetDocumentationResultsAsync(term);

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
                builder.Build();
                await ReplyAsync("", embed: builder);
            }
        }
    }
}
