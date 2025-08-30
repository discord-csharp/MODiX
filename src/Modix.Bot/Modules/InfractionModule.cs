#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.Options;

using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Common.Extensions;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Services.CommandHelp;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [ModuleHelp("Infractions", "Provides commands for working with infractions.")]
    public class InfractionModule : InteractionModuleBase
    {
        private readonly ModerationService _moderationService;
        private readonly ModixConfig _config;

        public InfractionModule(ModerationService moderationService, IOptions<ModixConfig> config)
        {
            _moderationService = moderationService;
            _config = config.Value;
        }

        [MessageCommand("Infractions")]
        [RequireClaims(AuthorizationClaim.ModerationRead)]
        // Discord doesn't currently give us many good options for default permissions.
        // Would be much better if the bot could set role-based defaults instead.
        // Goal is to at least make this not visible to ordinary users and visible to Associate+.
        [DefaultMemberPermissions(GuildPermission.ViewAuditLog)]
        public async Task SearchAsync(IMessage message)
            => await SearchAsync(message.Author);

        [SlashCommand("infractions", "Displays all non-deleted infractions for a user.")]
        [UserCommand("Infractions")]
        [RequireClaims(AuthorizationClaim.ModerationRead)]
        [DefaultMemberPermissions(GuildPermission.ViewAuditLog)]
        public async Task SearchAsync(
            [Summary(description: "The user whose infractions are to be displayed.")]
                IUser? user = null)
        {
            user ??= Context.User;

            var infractions = await _moderationService.SearchInfractionsAsync(
                new InfractionSearchCriteria
                {
                    GuildId = Context.Guild.Id,
                    SubjectId = user.Id,
                    IsDeleted = false
                },
                new[]
                {
                    new SortingCriteria()
                    {
                        PropertyName = nameof(InfractionSummary.CreateAction.Created),
                        Direction = SortDirection.Descending,
                    },
                });

            if (infractions.Count == 0)
            {
                await FollowupAsync(Format.Code("No infractions"), allowedMentions: AllowedMentions.None);
                return;
            }

            var infractionQuery = infractions.Select(infraction => new
            {
                Id = infraction.Id,
                Created = infraction.CreateAction.Created.ToUniversalTime().ToString("yyyy MMM dd"),
                Author = infraction.CreateAction.CreatedBy.GetFullUsername(),
                Type = infraction.Type,
                Reason = infraction.Reason,
                Rescinded = infraction.RescindAction != null
            });

            var counts = await _moderationService.GetInfractionCountsForUserAsync(user.Id);

            var url = new UriBuilder(_config.WebsiteBaseUrl)
            {
                Path = "/infractions",
                Query = $"subject={user.Id}"
            }.RemoveDefaultPort().ToString();

            var builder = new EmbedBuilder()
                .WithTitle($"Infractions for user: {user.GetDisplayName()}")
                .WithDescription(FormatUtilities.FormatInfractionCounts(counts))
                .WithUrl(url)
                .WithColor(new Color(0xA3BF0B));

            foreach (var infraction in infractionQuery)
            {
                // https://modix.gg/infractions?id=123
                var infractionUrl = new UriBuilder(_config.WebsiteBaseUrl)
                {
                    Path = "/infractions",
                    Query = $"id={infraction.Id}"
                }.ToString();

                var emoji = GetEmojiForInfractionType(infraction.Type);

                builder.AddField(
                    $"#{infraction.Id} - \\{emoji} {infraction.Type} - Created by {infraction.Author} on {infraction.Created}{(infraction.Rescinded ? " - [No longer in effect]" : "")}",
                    Format.Url($"Reason: {infraction.Reason}", infractionUrl)
                );
            }

            var embed = builder.Build();

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("infraction-delete", "Marks an infraction as deleted, so it no longer appears within infraction search results.")]
        [RequireClaims(AuthorizationClaim.ModerationDeleteInfraction)]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task DeleteAsync(
            [Summary(description: "The ID value of the infraction to be deleted.")]
                    long infractionId)
        {
            await _moderationService.DeleteInfractionAsync(infractionId);
            await Context.AddConfirmationAsync();
        }

        [SlashCommand("infraction-update", "Updates an infraction by ID, overwriting the existing reason.")]
        [RequireAnyClaim(AuthorizationClaim.ModerationUpdateInfraction, AuthorizationClaim.ModerationNote, AuthorizationClaim.ModerationWarn, AuthorizationClaim.ModerationMute, AuthorizationClaim.ModerationBan)]
        [DefaultMemberPermissions(GuildPermission.ViewAuditLog)]
        public async Task UpdateAsync(
            [Summary(description: "The ID value of the infraction to be update.")]
                    long infractionId,
            [Summary(description: "New reason for the infraction.")]
                    string newReason)
        {
            var (success, errorMessage) = await _moderationService.UpdateInfractionAsync(infractionId, newReason, Context.User.Id);

            if (!success)
            {
                var replyMessage = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Failed updating infraction."
                    : $"Failed updating infraction. {errorMessage}";

                await FollowupAsync(replyMessage, allowedMentions: AllowedMentions.None);
                return;
            }

            await Context.AddConfirmationAsync();
        }

        private static string GetEmojiForInfractionType(InfractionType infractionType)
            => infractionType switch
            {
                InfractionType.Notice => "📝",
                InfractionType.Warning => "⚠️",
                InfractionType.Mute => "🔇",
                InfractionType.Ban => "🔨",
                _ => "❔",
            };
    }
}
