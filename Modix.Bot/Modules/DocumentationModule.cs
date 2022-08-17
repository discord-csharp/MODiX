using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.Csharp;

namespace Modix.Modules
{
    [Name("Documentation")]
    [Summary("Search for information within the .NET docs.")]
    [HelpTags("docs")]
    public class DocumentationModule : ModuleBase
    {
        public DocumentationModule(DocumentationService documentationService,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            DocumentationService = documentationService;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

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
                    //Double the escape char so discord will print it as well
                    string s = (c == '\\') ? "\\\\" : c.ToString();
                    await ReplyAsync($" '{s}' character is not allowed in the search, please try again.");
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

                var urlText = $"{res.ItemType}: {res.DisplayName}";
                var url = "https://docs.microsoft.com" + res.Url.Replace("(", "%28").Replace(")", "%29");

                stringBuild.AppendLine(Format.Bold($"❯ {Format.Url(urlText, url)}"));
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

            var message = await ReplyAsync(embed: buildEmbed.Build());

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.User, buildEmbed, async (e) =>
            {
                await message.ModifyAsync(a =>
                {
                    a.Content = string.Empty;
                    a.Embed = e.Build();
                });
                return message;
            });

            await Context.Message.DeleteAsync();
        }

        protected DocumentationService DocumentationService { get; }
    }
}
