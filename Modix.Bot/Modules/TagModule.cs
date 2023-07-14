using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Options;
using Modix.Bot.Attributes;
using Modix.Bot.AutocompleteHandlers;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Tags;
using Modix.Services.Utilities;

namespace Modix.Bot.Modules
{
    [ModuleHelp("Tags", "Use and maintain tags.")]
    [Group("tag", "Use and maintain tags.")]
    public class TagModule : InteractionModuleBase
    {
        public TagModule(
            ITagCache tagCache,
            ITagService tagService,
            IUserService userService,
            IOptions<ModixConfig> config)
        {
            _tagCache = tagCache;
            _tagService = tagService;
            _userService = userService;
            _config = config.Value;
        }

        [SlashCommand("create", "Creates a new tag.")]
        [RequireClaims(AuthorizationClaim.CreateTag)]
        [DoNotDefer]
        public async Task CreateTagAsync()
        {
            await RespondWithModalAsync<TagCreateModal>("modal_tag_create");
        }

        [ModalInteraction("modal_tag_create", ignoreGroupNames: true)]
        public async Task CreateTagFromModalResponseAsync(TagCreateModal modal)
        {
            await _tagService.CreateTagAsync(Context.Guild.Id, Context.User.Id, modal.Name, modal.Content);
            await Context.AddConfirmationAsync($"Added tag '{modal.Name}'.");
        }

        [SlashCommand("update", "Updates the contents of a tag.")]
        [DoNotDefer]
        public async Task ModifyTagAsync(
            [Summary(description: "The name that is used to invoke the tag.")]
            [Autocomplete(typeof(TagAutocompleteHandler))]
                string name)
        {
            name = name.Trim().ToLower();

            var currentTagData = await _tagService.GetTagAsync(Context.Guild.Id, name);

            if (currentTagData is null)
            {
                await RespondAsync($"The tag '{name}' does not exist.", allowedMentions: AllowedMentions.None);
                return;
            }

            await Context.Interaction.RespondWithModalAsync<TagUpdateModal>("modal_tag_update", modifyModal: modal =>
            {
                modal.AddTextInput("Name", "tag_name", required: true, value: name);
                modal.AddTextInput("New content", "tag_content", style: TextInputStyle.Paragraph, required: true, value: currentTagData.Content);
            });
        }

        [ModalInteraction("modal_tag_update", ignoreGroupNames: true)]
        public async Task UpdateTagFromModalResponseAsync(TagUpdateModal modal)
        {
            var name = ((IModalInteraction)Context.Interaction).Data.Components.Single(x => x.CustomId == "tag_name").Value;
            var content = ((IModalInteraction)Context.Interaction).Data.Components.Single(x => x.CustomId == "tag_content").Value;
            await _tagService.ModifyTagAsync(Context.Guild.Id, Context.User.Id, name, content);
            await Context.AddConfirmationAsync($"Updated tag '{name}'.");
        }

        [SlashCommand("delete", "Deletes a tag.")]
        public async Task DeleteTagAsync(
            [Summary(description: "The name that is used to invoke the tag.")]
            [Autocomplete(typeof(TagAutocompleteHandler))]
                string name)
        {
            await _tagService.DeleteTagAsync(Context.Guild.Id, Context.User.Id, name);
            await Context.AddConfirmationAsync($"Deleted tag '{name}'.");
        }

        [SlashCommand("ownedby", "Lists all tags owned by the supplied user or role.")]
        public async Task OwnedByAsync(
            [Summary(description: "The user or role whose tags are to be retrieved. If left blank, the current user.")]
                IMentionable owner = null)
        {
            owner ??= Context.User;

            if (owner is IUser ownerUser)
            {
                var userId = ownerUser.Id;

                var tags = await _tagService.GetTagsOwnedByUserAsync(Context.Guild.Id, userId);

                var user = await _userService.GetUserAsync(userId);

                var embed = BuildEmbed(tags, ownerUser: user);

                await FollowupAsync(embed: embed);
            }
            else if (owner is IRole ownerRole)
            {
                var tags = await _tagService.GetTagsOwnedByRoleAsync(Context.Guild.Id, ownerRole.Id);

                var embed = BuildEmbed(tags, ownerRole: ownerRole);

                await FollowupAsync(embed: embed);
            }
            else
            {
                await FollowupAsync($"Unable to identify {owner.Mention} as a user or role.", allowedMentions: AllowedMentions.None);
            }
        }

        [SlashCommand("owner", "Lists the owner of the supplied tag.")]
        public async Task GetOwnerAsync(
            [Summary(description: "The name of the tag whose owner is being requested.")]
            [Autocomplete(typeof(TagAutocompleteHandler))]
                string name)
        {
            var tag = await _tagService.GetTagAsync(Context.Guild.Id, name);

            if (tag is null)
            {
                await FollowupAsync($"The tag '{name.Trim().ToLower()}' does not exist.", allowedMentions: AllowedMentions.None);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithDescription($"Owns the '{tag.Name}' tag.")
                .WithColor(Color.DarkPurple)
                .WithFooter(EmbedFooterText);

            if (tag.OwnerUser != null)
            {
                var user = await _userService.GetUserAsync(tag.OwnerUser.Id);

                if (user != null)
                {
                    embedBuilder.WithUserAsAuthor(user, user.Id.ToString());
                }
                else
                {
                    embedBuilder.WithAuthor($"{tag.OwnerUser.GetFullUsername()} ({tag.OwnerUser.Id})");
                }
            }
            else if (tag.OwnerRole != null)
            {
                var role = Context.Guild.GetRole(tag.OwnerRole.Id);

                embedBuilder
                    .WithAuthor($"The {role.Name} role", Context.Guild.IconUrl)
                    .WithColor(role.Color);
            }
            else
            {
                embedBuilder
                    .WithDescription($"Unable to find the owner of the '{tag.Name}' tag.")
                    .WithColor(Color.Red);
            }

            await FollowupAsync(embed: embedBuilder.Build());
        }

        [SlashCommand("list", "Lists all tags available in the current guild.")]
        public async Task ListAllAsync()
        {
            var tags = await _tagService.GetSummariesAsync(new TagSearchCriteria()
            {
                GuildId = Context.Guild.Id
            });

            var embed = BuildEmbed(tags, ownerGuild: Context.Guild);

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("transfer", "Transfers ownership of a tag to the supplied user or role.")]
        public async Task TransferAsync(
            [Summary(description: "The name of the tag to be transferred.")]
            [Autocomplete(typeof(TagAutocompleteHandler))]
                string name,
            [Summary(description: "The user or role to whom the tag should be transferred.")]
                IMentionable target)
        {
            if (target is IUser user)
            {
                await _tagService.TransferToUserAsync(Context.Guild.Id, name, Context.User.Id, user.Id);
            }
            else if (target is IRole role)
            {
                await _tagService.TransferToRoleAsync(Context.Guild.Id, name, Context.User.Id, role.Id);
            }
            else
            {
                await FollowupAsync($"Unable to identify {target.Mention} as a user or role.", allowedMentions: AllowedMentions.None);
                return;
            }

            await Context.AddConfirmationAsync();
        }

        [SlashCommand("search", "Searches for a tag based on a partial name.")]
        public async Task SearchAsync(
            [Summary(description: "Partial name of the tag to search for.")]
                string partialName)
        {
            partialName = partialName.Trim();

            var tags = _tagCache.Search(Context.Guild.Id, partialName);
            var tagsForEmbed = tags
                .Take(EmbedBuilder.MaxFieldCount)
                .Select(x => $"• {x}")
                .ToArray();

            var embed = new EmbedBuilder()
                .WithTitle("Tag Search Results")
                .WithDescription($"Tags matching the search query \"{Format.Sanitize(partialName)}\":\n{string.Join('\n', tagsForEmbed)}")
                .WithFooter($"{tagsForEmbed.Length} of {tags.Length} results shown");

            await FollowupAsync(embed: embed.Build());
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
                .WithFooter(EmbedFooterText)
                .WithTitle("Tags");

            const int TagsToDisplay = 6;

            foreach (var tag in orderedTags.Take(TagsToDisplay))
            {
                builder.AddField(tag.Name, $"{tag.Uses} uses", true);
            }

            if (tags.Count > TagsToDisplay)
            {
                var fieldName = $"and {tags.Count - TagsToDisplay} more";

                // https://modix.gg/tags
                var url = new UriBuilder(_config.WebsiteBaseUrl)
                {
                    Path = "/tags"
                }.RemoveDefaultPort().ToString();

                builder.AddField(x => x.WithName(fieldName)
                                       .WithValue($"View at {url}"));
            }

            return builder.Build();
        }

        private const string EmbedFooterText = "Use tags inline with \"$name\"";
        private readonly ITagCache _tagCache;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;
        private readonly ModixConfig _config;
    }

    public class TagCreateModal : IModal
    {
        public string Title => "New Tag";

        [ModalTextInput("tag_name")]
        [RequiredInput]
        public string Name { get; set; }

        [ModalTextInput("tag_content", TextInputStyle.Paragraph)]
        [RequiredInput]
        public string Content { get; set; }
    }

    public class TagUpdateModal : IModal
    {
        public string Title => "Updating Tag";
    }
}
