using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CommandHelp;
using Modix.Services.Csharp;

namespace Modix.Modules
{
    [ModuleHelp("Documentation", "Search for information within the .NET docs.")]
    [HelpTags("docs")]
    public class DocumentationModule : InteractionModuleBase
    {
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;
        private readonly DocumentationService _documentationService;

        // lang=regex
        private const string QueryPattern = "^[0-9A-Za-z.<>]$";

        public DocumentationModule(DocumentationService documentationService,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            _documentationService = documentationService;
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        [SlashCommand("docs", "Shows class/method reference from the new unified .NET reference.")]
        public async Task GetDocumentationAsync(
            [Summary(description: "The term to search for in the documentation.")]
                string term)
        {
            var reg = new Regex(QueryPattern);
            foreach (var c in term)
            {
                if(!reg.IsMatch(c.ToString()))
                {
                    //Double the escape char so discord will print it as well
                    var s = (c == '\\') ? "\\\\" : c.ToString();
                    await FollowupAsync($"'{s}' character is not allowed in the search, please try again. Query must match the following regex: `{QueryPattern}`", allowedMentions: AllowedMentions.None);
                    return;
                }
            }

            var response = await _documentationService.GetDocumentationResultsAsync(term);

            if (response.Count == 0)
            {
                await FollowupAsync("Could not find documentation for your requested term.");
                return;
            }

            var embedCount = 0;
            var stringBuild = new StringBuilder();

            foreach (var res in response.Results.Take(3))
            {
                embedCount++;

                var urlText = $"{res.ItemType}: {res.DisplayName}";
                var url = $"https://docs.microsoft.com{res.Url.Replace("(", "%28").Replace(")", "%29")}";

                stringBuild.AppendLine(Format.Bold($"❯ {Format.Url(urlText, url)}"));
                stringBuild.AppendLine(res.Description);
                stringBuild.AppendLine();

                if (embedCount == 3)
                {
                    stringBuild.Append(
                        $"{embedCount}/{response.Results.Count} results shown ~ [click here for more results](https://docs.microsoft.com/dotnet/api/?term={term})"
                    );
                }
            }

            var buildEmbed = new EmbedBuilder()
            {
                Description = stringBuild.ToString()
            }.WithColor(new Color(46, 204, 113));

            var message = await FollowupAsync(embed: buildEmbed.Build());

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(Context.User, buildEmbed, async (e) =>
            {
                await message.ModifyAsync(a =>
                {
                    a.Content = string.Empty;
                    a.Embed = e.Build();
                });
                return message;
            });
        }
    }
}
