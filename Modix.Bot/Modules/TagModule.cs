using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Services.CodePaste;
using Modix.Services.Core;
using Modix.Services.Tags;
using Modix.Services.Utilities;

namespace Modix.Bot.Modules
{
    [Name("Tags")]
    [Summary("Use and maintain tags.")]
    [Group("tag")]
    [Alias("tags")]
    public class TagModule : ModuleBase
    {
        public TagModule(
            ITagService tagService,
            CodePasteService codePasteService,
            IUserService userService,
            IOptions<ModixConfig> config)
        {
            TagService = tagService;
            CodePasteService = codePasteService;
            UserService = userService;
            Config = config.Value;
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
        {
            if (await TagService.TagExistsAsync(Context.Guild.Id, name) == false)
            {
                await HandleTagError($"Couldn't find tag \"{name}\" in this guild.");
                return;
            }

            try
            {
                await TagService.UseTagAsync(Context.Guild.Id, Context.Channel.Id, name);
            }
            catch (InvalidOperationException ex)
            {
                await HandleTagError(ex.Message);
            }
        }

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

        [Command("ownedby")]
        [Alias("ownedby me")]
        [Summary("Lists all tags owned by the supplied user.")]
        public async Task ListAsync(
            [Summary("The user whose tags are to be retrieved. If left blank, the current user.")]
                DiscordUserEntity discordUser = null)
        {
            var userId = discordUser?.Id ?? Context.User.Id;

            var tags = await TagService.GetTagsOwnedByUserAsync(Context.Guild.Id, userId);

            var user = await UserService.GetUserInformationAsync(Context.Guild.Id, userId);

            var embed = BuildEmbed(tags, ownerUser: user);

            await ReplyAsync(embed: embed);
        }

        [Command("ownedby")]
        [Summary("Lists all tags owned by the supplied role.")]
        public async Task ListAsync(
            [Summary("The role whose tags are to be retrieved.")]
                IRole role)
        {
            var tags = await TagService.GetTagsOwnedByRoleAsync(Context.Guild.Id, role.Id);

            var embed = BuildEmbed(tags, ownerRole: role);

            await ReplyAsync(embed: embed);
        }

        [Command("all")]
        [Alias("list", "")]
        [Summary("Lists all tags available in the current guild.")]
        public async Task ListAllAsync()
        {
            var tags = await TagService.GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = Context.Guild.Id
            });

            var embed = BuildEmbed(tags, ownerGuild: Context.Guild);

            await ReplyAsync(embed: embed);
        }

        [Command("transfer")]
        [Summary("Transfers ownership of a tag to the supplied user.")]
        public async Task TransferToUserAsync(
            [Summary("The name of the tag to be transferred.")]
                string name,
            [Summary("The user to whom the tag should be transferred.")]
                DiscordUserEntity target)
        {
            await TagService.TransferToUserAsync(Context.Guild.Id, name, Context.User.Id, target.Id);
            await Context.AddConfirmation();
        }

        [Command("transfer")]
        [Summary("Transfers ownership of a tag to the supplied role.")]
        public async Task TransferToRoleAsync(
            [Summary("The name of the tag to be transferred.")]
                string name,
            [Summary("The role to which the tag should be transferred.")]
                IRole target)
        {
            await TagService.TransferToRoleAsync(Context.Guild.Id, name, Context.User.Id, target.Id);
            await Context.AddConfirmation();
        }

        protected CodePasteService CodePasteService { get; }

        protected ITagService TagService { get; }

        protected IUserService UserService { get; }

        protected ModixConfig Config { get; }

        private async Task HandleTagError(string message)
        {
            var embed = new EmbedBuilder()
                        .WithTitle("Error")
                        .WithColor(Color.Red)
                        .WithDescription(message);

            await ReplyAsync(embed: embed.Build());
        }

        private Embed BuildEmbed(IReadOnlyCollection<TagSummary> tags, IUser ownerUser = null, IGuild ownerGuild = null, IRole ownerRole = null)
        {
            var orderedTags = tags
                .OrderByDescending(x => x.Uses)
                .ThenBy(x => x.Name);

            var ownerName = ownerUser?.Username
                ?? ownerGuild?.Name
                ?? ownerRole?.Name;

            var ownerImage = ownerUser?.GetDefiniteAvatarUrl()
                ?? ownerGuild?.IconUrl;

            var builder = new EmbedBuilder();

            builder
                .WithAuthor(ownerName, ownerImage)
                .WithColor(Color.DarkPurple)
                .WithDescription(tags.Count > 0 ? null : "No tags.")
                .WithFooter("Use tags with \"!tag name\", or inline with \"$name\"")
                .WithTitle("Tags");

            const int tagsToDisplay = 6;

            foreach (var tag in orderedTags.Take(tagsToDisplay))
            {
                builder.AddField(tag.Name, $"{tag.Uses} uses", true);
            }

            if (tags.Count > tagsToDisplay)
            {
                var fieldName = $"and {tags.Count - tagsToDisplay} more";

                // https://modix.gg/tags
                var url = new UriBuilder(Config.WebsiteBaseUrl)
                {
                    Path = "/tags"
                }.ToString(true);

                builder.AddField(x => x.WithName(fieldName)
                                       .WithValue($"View at {url}"));
            }

            return builder.Build();
        }
    }
}
