using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Modix.Bot.Extensions;
using Modix.Data.Models.Tags;
using Modix.Services.AutoCodePaste;
using Modix.Services.Core;
using Modix.Services.Tags;

namespace Modix.Bot.Modules
{
    [Name("Tags")]
    [Summary("Use and maintain tags.")]
    [Group("tag")]
    [Alias("tags")]
    public class TagModule : ModuleBase
    {
        public TagModule(ITagService tagService, CodePasteService codePasteService, IUserService userService)
        {
            TagService = tagService;
            CodePasteService = codePasteService;
            UserService = userService;
        }

        [Command("create")]
        [Alias("add")]
        [Summary("Creates a new tag.")]
        public async Task CreateTagAsync(
            [Summary("The name that will be used to invoke the tag.")]
                string name,
            [Remainder]
            [Summary("The message that will be displayed when the tag is invoked.")]
                string content)
        {
            await TagService.CreateTagAsync(Context.Guild.Id, Context.User.Id, name, content);
            await Context.AddConfirmation();
        }

        [Command]
        [Priority(-10)]
        [Summary("Invokes a tag so that the message associated with the tag will be displayed.")]
        public async Task UseTagAsync(
            [Summary("The name that will be used to invoke the tag.")]
                string name)
            => await TagService.UseTagAsync(Context.Guild.Id, Context.Channel.Id, name);

        [Command("update")]
        [Alias("edit", "modify")]
        [Summary("Updates the contents of a tag.")]
        public async Task ModifyTagAsync(
            [Summary("The name that is used to invoke the tag.")]
                string name,
            [Remainder]
            [Summary("The new message that will be displayed when the tag is invoked.")]
                string newContent)
        {
            await TagService.ModifyTagAsync(Context.Guild.Id, Context.User.Id, name, newContent);
            await Context.AddConfirmation();
        }

        [Command("delete")]
        [Alias("remove")]
        [Summary("Deletes a tag.")]
        public async Task DeleteTagAsync(
            [Summary("The name that is used to invoke the tag.")]
                string name)
        {
            await TagService.DeleteTagAsync(Context.Guild.Id, Context.User.Id, name);
            await Context.AddConfirmation();
        }

        [Command("list")]
        [Summary("Lists all tags owned by the user.")]
        public async Task ListAsync()
        {
            var tags = await TagService.GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = Context.Guild.Id,
                CreatedById = Context.User.Id,
            });

            var embed = await BuildEmbedAsync(tags, ownerUser: Context.User);

            await ReplyAsync(embed: embed);
        }

        [Command("list")]
        [Summary("Lists all tags owned by the supplied user.")]
        public async Task ListAsync(
            [Summary("The user whose tags are to be retrieved")]
                DiscordUserEntity discordUser)
        {
            var tags = await TagService.GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = Context.Guild.Id,
                CreatedById = discordUser.Id,
            });

            var user = await UserService.GetUserInformationAsync(Context.Guild.Id, discordUser.Id);

            var embed = await BuildEmbedAsync(tags, ownerUser: user);

            await ReplyAsync(embed: embed);
        }

        [Command("list all")]
        [Summary("Lists all tags available in the current guild.")]
        public async Task ListAllAsync()
        {
            var tags = await TagService.GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = Context.Guild.Id,
            });

            var embed = await BuildEmbedAsync(tags, ownerGuild: Context.Guild);

            await ReplyAsync(embed: embed);
        }

        protected CodePasteService CodePasteService { get; }

        protected ITagService TagService { get; }

        protected IUserService UserService { get; }

        private async Task<Embed> BuildEmbedAsync(IReadOnlyCollection<TagSummary> tags, IUser ownerUser = null, IGuild ownerGuild = null)
        {
            var orderedTags = tags.OrderBy(x => x.Name);

            var ownerName = ownerUser?.Username ?? ownerGuild?.Name;
            var ownerImage = ownerUser?.GetAvatarUrl() ?? ownerGuild?.IconUrl;

            var builder = new EmbedBuilder();

            builder
                .WithAuthor(ownerName, ownerImage)
                .WithColor(Color.DarkPurple)
                .WithDescription(tags.Count > 0 ? null : "No tags.")
                .WithTimestamp(DateTimeOffset.Now)
                .WithTitle("Tags");

            const int tagsToDisplay = 5;

            foreach (var tag in orderedTags.Take(tagsToDisplay))
            {
                builder.AddField(x => x.WithName(tag.Name)
                                       .WithValue($"{tag.Uses} uses"));
            }

            if (tags.Count > tagsToDisplay)
            {
                var pasteContent = BuildPaste(orderedTags);

                var fieldName = $"and {tags.Count - tagsToDisplay} more";

                try
                {
                    var pasteLink = await CodePasteService.UploadCodeAsync(pasteContent, "txt");

                    builder.AddField(x => x.WithName(fieldName)
                                           .WithValue($"[View at {pasteLink}]({pasteLink})"));
                }
                catch (WebException ex)
                {
                    builder.AddField(x => x.WithName(fieldName)
                                           .WithValue(ex.Message));
                }
            }

            return builder.Build();
        }

        private string BuildPaste(IOrderedEnumerable<TagSummary> tags)
        {
            var builder = new StringBuilder();

            foreach (var tag in tags)
            {
                builder.AppendLine($"{tag.Name} ({tag.Uses} uses)");
            }

            return builder.ToString();
        }
    }
}
