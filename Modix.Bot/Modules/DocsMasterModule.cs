using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.DocsMaster;
using Modix.Services.DocsMaster.Models;

namespace Modix.Modules
{
    [Group("docsmaster"), Name("DocsMaster"), Summary("Search for information from many documentations.")]
    public class DocsMasterModule : ModuleBase
    {
        private readonly DocsMasterRetrievalService _docsService;

        public DocsMasterModule(DocsMasterRetrievalService docsService)
        {
            _docsService = docsService;
        }

        [Command("find")]
        public async Task Find(string manualName, string entryName, string version = "")
        {
            ManualEntryModel resultModel;
            if (string.IsNullOrWhiteSpace(version))
            {
                resultModel = await _docsService.GetManualEntryFromLatestAsync(manualName, entryName);
            }
            else
            {
                resultModel = await _docsService.GetManualEntryFromVersionAsync(manualName, entryName, version);
            }

            if (string.IsNullOrWhiteSpace(resultModel.EntryName))
            {
                await ReplyAsync("No such manual or manual entry exists.");
                return;
            }

            var builder = new EmbedBuilder().WithAuthor(new EmbedAuthorBuilder {Name = resultModel.EntryName})
                .WithThumbnailUrl(
                    Context.Client.CurrentUser.GetAvatarUrl())
                .AddField("Description:", resultModel.Description)
                .AddField("Full Reference:", resultModel.FullReferenceLink)
                .WithColor(Color.Green);
            await ReplyAsync("", embed: builder.Build());
        }

        [Command("versions")]
        public async Task ListVersions(string manualName)
        {
            var builder = new EmbedBuilder().WithAuthor("CURRENT SUPPORTED VERSIONS");
            var sb = new StringBuilder();
            foreach (var version in await _docsService.GetAllVersionsAsync(manualName))
            {
                sb.AppendLine(version);
            }

            builder.Description = sb.ToString();

            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl()).WithColor(Color.Green);
            await ReplyAsync("", embed: builder.Build());
        }
    }
}
