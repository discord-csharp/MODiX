#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Humanizer;
using Humanizer.Localisation;

using Microsoft.Extensions.Options;

using Modix.Bot.Attributes;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Utilities;
using Modix.Services.CommandHelp;
using Modix.Services.Promotions;
using Modix.Services.Utilities;

using Serilog;

namespace Modix.Modules
{
    [ModuleHelp("Promotions", "Manage promotion campaigns.")]
    [HelpTags("campaigns", "nominate", "nomination", "nominating")]
    public class PromotionsModule(IOptions<ModixConfig> config, IPromotionsService promotionsService) : InteractionModuleBase
    {
        private readonly ModixConfig _config = config.Value;

        [SlashCommand("promotions", "List all active promotion campaigns.")]
        [RequireClaims(AuthorizationClaim.PromotionsRead)]
        [DefaultMemberPermissions((GuildPermission)(1UL << 43))]
        public async Task CampaignsAsync()
        {
            var campaigns = await promotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria()
            {
                GuildId = Context.Guild.Id,
                IsClosed = false
            });

            if (campaigns.Count == 0)
            {
                await FollowupAsync("There are no active promotion campaigns.");
                return;
            }

            foreach (var campaign in campaigns)
            {
                var embed = GetCampaignEmbed(campaign);
                var components = GetCampaignComponents(campaign);

                await FollowupAsync(embed: embed, components: components);
            }
        }

        [SlashCommand("promotions-nominate", "Nominate the given user for promotion.")]
        [RequireClaims(AuthorizationClaim.PromotionsCreateCampaign)]
        [DefaultMemberPermissions((GuildPermission)(1UL << 43))]
        public async Task NominateAsync(
            [Summary(description: "The user to nominate.")]
                IGuildUser subject)
        {
            var targetRole = await promotionsService.GetNextRankRoleForUserAsync(subject.Id);
            if (targetRole is null)
            {
                await FollowupAsync($"There are no roles for {subject.Mention} to be promoted to.", allowedMentions: AllowedMentions.None);
                return;
            }

            var existingCampaign = (await promotionsService.SearchCampaignsAsync(new()
            {
                GuildId = Context.Guild.Id,
                SubjectId = subject.Id,
                IsClosed = false,
            })).FirstOrDefault();

            if (existingCampaign is not null)
            {
                await FollowupAsync($"There is already an active campaign ({existingCampaign.Id}) for {subject.Mention} to be promoted to {MentionUtils.MentionRole(targetRole.Id)}.", allowedMentions: AllowedMentions.None);
                return;
            }
            
            await Context.GetUserConfirmationAsync(
                $"You are nominating {subject.Mention} for promotion to {MentionUtils.MentionRole(targetRole.Id)}.",
                customIdSuffix: $"nominate:{Context.User.Id},{subject.Id}");
        }

        [ComponentInteraction("button_confirm_nominate:*,*")]
        [DoNotDefer]
        public async Task ConfirmNominationAsync(ulong nominatorId, ulong subjectId)
        {
            var interaction = (IComponentInteraction)Context.Interaction;

            if (interaction.User.Id != nominatorId)
            {
                await DeferAsync();
                return;
            }

            Context.StopMonitoringConfirmationDialog(interaction.Message);

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = "✅ Confirmed. Please either enter a comment or submit a blank comment to finalize the nomination.";
                x.Embed = null;
                x.Components = null;
            });

            var subject = await Context.Guild.GetUserAsync(subjectId);

            var modal = new PromotionCommentModal
            {
                Title = $"Comment for {subject?.GetDisplayName() ?? "campaign"}".Truncate(45),
            };

            await interaction.RespondWithModalAsync($"modal_campaign_nominate:{subjectId}", modal);
        }

        [ModalInteraction("modal_campaign_nominate:*")]
        [EphemeralErrors]
        public async Task ApproveFromModalResponseAsync(ulong subjectId, PromotionCommentModal modal)
        {
            var interaction = (IModalInteraction)Context.Interaction;

            await promotionsService.CreateCampaignAsync(subjectId, modal.Content);

            var campaign = (await promotionsService.SearchCampaignsAsync(new()
            {
                GuildId = Context.Guild.Id,
                SubjectId = subjectId,
                IsClosed = false,
            })).SingleOrDefault();

            var embed = GetCampaignEmbed(campaign);
            var components = GetCampaignComponents(campaign);

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = null;
                x.Embed = embed;
                x.Components = components;
            });
        }

        [ComponentInteraction("button_cancel_nominate:*,*")]
        public async Task CancelNominationAsync(ulong nominatorId, ulong subjectId)
        {
            var interaction = (IComponentInteraction)Context.Interaction;

            if (interaction.User.Id != nominatorId)
                return;

            Context.StopMonitoringConfirmationDialog(interaction.Message);

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = "❌ Canceled.";
                x.Embeds = null;
                x.Components = null;
            });
        }

        [ComponentInteraction("button_campaign_approve:*")]
        [DoNotDefer]
        public async Task ApproveAsync(long campaignId)
        {
            var campaign = await promotionsService.GetCampaignDetailsAsync(campaignId);
            if (campaign is null)
            {
                await RespondAsync($"Could not find a campaign with ID {campaignId}.", ephemeral: true, allowedMentions: AllowedMentions.None);
                return;
            }

            var existingComment = (await promotionsService.SearchPromotionCommentsAsync(new()
            {
                CampaignId = campaignId,
                CreatedById = Context.Interaction.User.Id,
                IsModified = false,
            }))
            .SingleOrDefault();

            if (campaign.Outcome is not null)
            {
                var interaction = (IComponentInteraction)Context.Interaction;
                await RespondAsync($"Campaign {campaignId} has already been closed.", ephemeral: true, allowedMentions: AllowedMentions.None);
                await RefreshCampaignMessageAsync(interaction.Message, campaignId);
                return;
            }

            var subject = await Context.Guild.GetUserAsync(campaign.Subject.Id);

            var modal = new PromotionCommentModal
            {
                Title = $"Comment for {subject?.GetDisplayName() ?? "campaign"}".Truncate(45),
                Content = existingComment?.Content,
            };

            await Context.Interaction.RespondWithModalAsync($"modal_campaign_approve:{campaignId}", modal);
        }

        [ModalInteraction("modal_campaign_approve:*")]
        [EphemeralErrors]
        public async Task ApproveFromModalResponseAsync(long campaignId, PromotionCommentModal modal)
        {
            await promotionsService.AddOrUpdateCommentAsync(campaignId, PromotionSentiment.Approve, modal.Content);
            await FollowupAsync("✅ Successfully cast a vote to approve the promotion.", ephemeral: true);
            await RefreshCampaignMessageAsync(((IModalInteraction)Context.Interaction).Message, campaignId);
        }

        [ComponentInteraction("button_campaign_oppose:*")]
        [DoNotDefer]
        public async Task OpposeAsync(long campaignId)
        {
            var campaign = await promotionsService.GetCampaignDetailsAsync(campaignId);
            if (campaign is null)
            {
                await RespondAsync($"Could not find a campaign with ID {campaignId}.", ephemeral: true, allowedMentions: AllowedMentions.None);
                return;
            }

            var existingComment = (await promotionsService.SearchPromotionCommentsAsync(new()
            {
                CampaignId = campaignId,
                CreatedById = Context.Interaction.User.Id,
                IsModified = false,
            }))
            .SingleOrDefault();

            if (campaign.Outcome is not null)
            {
                var interaction = (IComponentInteraction)Context.Interaction;
                await RespondAsync($"Campaign {campaignId} has already been closed.", ephemeral: true, allowedMentions: AllowedMentions.None);
                await RefreshCampaignMessageAsync(interaction.Message, campaignId);
                return;
            }

            var subject = await Context.Guild.GetUserAsync(campaign.Subject.Id);

            var modal = new PromotionCommentModal
            {
                Title = $"Comment for {subject?.GetDisplayName() ?? "campaign"}".Truncate(45),
                Content = existingComment?.Content,
            };

            await Context.Interaction.RespondWithModalAsync($"modal_campaign_oppose:{campaignId}", modal);
        }

        [ModalInteraction("modal_campaign_oppose:*")]
        [EphemeralErrors]
        public async Task OpposeFromModalResponseAsync(long campaignId, PromotionCommentModal modal)
        {
            await promotionsService.AddOrUpdateCommentAsync(campaignId, PromotionSentiment.Oppose, modal.Content);
            await FollowupAsync("✅ Successfully cast a vote to oppose the promotion.", ephemeral: true);
            await RefreshCampaignMessageAsync(((IModalInteraction)Context.Interaction).Message, campaignId);
        }

        [ComponentInteraction("button_campaign_comment:*")]
        [DoNotDefer]
        public async Task CommentAsync(long campaignId)
        {
            var campaign = await promotionsService.GetCampaignDetailsAsync(campaignId);
            if (campaign is null)
            {
                await RespondAsync($"Could not find a campaign with ID {campaignId}.", ephemeral: true, allowedMentions: AllowedMentions.None);
                return;
            }

            var existingComment = (await promotionsService.SearchPromotionCommentsAsync(new()
            {
                CampaignId = campaignId,
                CreatedById = Context.Interaction.User.Id,
                IsModified = false,
            }))
            .SingleOrDefault();

            if (existingComment is null)
            {
                await RespondAsync("Could not find an existing vote. Please cast a vote first.", ephemeral: true);
                return;
            }

            if (campaign.Outcome is not null)
            {
                var interaction = (IComponentInteraction)Context.Interaction;
                await RespondAsync($"Campaign {campaignId} has already been closed.", ephemeral: true, allowedMentions: AllowedMentions.None);
                await RefreshCampaignMessageAsync(interaction.Message, campaignId);
                return;
            }

            var subject = await Context.Guild.GetUserAsync(campaign.Subject.Id);

            var modal = new PromotionCommentModal
            {
                Title = $"Comment for {subject?.GetDisplayName() ?? "campaign"}".Truncate(45),
                Content = existingComment.Content,
            };

            await Context.Interaction.RespondWithModalAsync($"modal_campaign_comment:{campaignId}", modal);
        }

        [ModalInteraction("modal_campaign_comment:*")]
        [EphemeralErrors]
        public async Task UpdateCommentFromModalResponseAsync(long campaignId, PromotionCommentModal modal)
        {
            await promotionsService.AddOrUpdateCommentAsync(campaignId, sentiment: default, comment: modal.Content);
            await FollowupAsync("✅ Successfully updated the comment.", ephemeral: true);
            await RefreshCampaignMessageAsync(((IModalInteraction)Context.Interaction).Message, campaignId);
        }

        [ComponentInteraction("button_campaign_refresh:*")]
        public async Task RefreshAsync(long campaignId)
        {
            await RefreshCampaignMessageAsync(((IComponentInteraction)Context.Interaction).Message, campaignId);
        }

        [SlashCommand("promotions-accept", "Accept an ongoing campaign to promote a user, and perform the promotion.")]
        [RequireClaims(AuthorizationClaim.PromotionsCloseCampaign)]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task AcceptAsync(
            [Summary(description: "The ID value of the campaign to be accepted.")]
                long campaignId)
        {
            var campaign = (await promotionsService.SearchCampaignsAsync(new() { Id = campaignId })).SingleOrDefault();
            if (campaign is null || campaign.Outcome is PromotionCampaignOutcome.Accepted or PromotionCampaignOutcome.Rejected)
            {
                await FollowupAsync($"Error: Campaign {campaignId} doesn't exist or is already closed.", allowedMentions: AllowedMentions.None);
                return;
            }

            var timeRemaining = campaign.GetTimeUntilCampaignCanBeClosed();

            if (timeRemaining.TotalMinutes > 1)
            {
                await Context.GetUserConfirmationAsync(
                    $"Campaign {campaignId} still has {timeRemaining.Humanize(precision: 2, minUnit: TimeUnit.Minute)} remaining. Are you sure you want to accept it?",
                    $"accept:{campaignId},{Context.User.Id}");
                return;
            }

            await promotionsService.AcceptCampaignAsync(campaignId, true);
            await Context.AddConfirmationAsync();
        }

        [ComponentInteraction("button_confirm_accept:*,*")]
        public async Task ConfirmAcceptAsync(long campaignId, ulong commandUserId)
        {
            var interaction = (IComponentInteraction)Context.Interaction;

            if (interaction.User.Id != commandUserId)
                return;

            Context.StopMonitoringConfirmationDialog(interaction.Message);

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = "✅ Confirmed. Accepting the campaign.";
                x.Embed = null;
                x.Components = null;
            });

            await promotionsService.AcceptCampaignAsync(campaignId, true);
        }

        [ComponentInteraction("button_cancel_accept:*,*")]
        public async Task CancelAcceptAsync(long campaignId, ulong commandUserId)
        {
            var interaction = (IComponentInteraction)Context.Interaction;

            if (interaction.User.Id != commandUserId)
                return;

            Context.StopMonitoringConfirmationDialog(interaction.Message);

            await interaction.Message.ModifyAsync(x =>
            {
                x.Content = "❌ Canceled.";
                x.Embeds = null;
                x.Components = null;
            });
        }

        [SlashCommand("promotions-reject", "Reject an ongoing campaign to promote a user.")]
        [RequireClaims(AuthorizationClaim.PromotionsCloseCampaign)]
        [DefaultMemberPermissions(GuildPermission.BanMembers)]
        public async Task RejectAsync(
            [Summary(description: "The ID value of the campaign to be rejected.")]
                long campaignId)
        {
            await promotionsService.RejectCampaignAsync(campaignId);
            await Context.AddConfirmationAsync();
        }

        private async Task RefreshCampaignMessageAsync(IUserMessage message, long campaignId)
        {
            var campaign = (await promotionsService.SearchCampaignsAsync(new() { Id = campaignId })).SingleOrDefault();
            if (campaign is null)
            {
                Log.Error("Could not find a campaign with ID {CampaignId}.", campaignId);
                return;
            }

            var embed = GetCampaignEmbed(campaign);

            await message.ModifyAsync(x =>
            {
                x.Embed = embed;
                x.Components = campaign.Outcome is null
                    ? GetCampaignComponents(campaign)
                    : null;
            });
        }

        [return: NotNullIfNotNull(nameof(campaign))]
        private Embed? GetCampaignEmbed(PromotionCampaignSummary? campaign)
        {
            if (campaign is null)
                return null;

            // https://mod.gg/promotions
            var url = new UriBuilder(_config.WebsiteBaseUrl)
            {
                Path = "/promotions"
            }.RemoveDefaultPort().ToString();

            var totalVotes = campaign.ApproveCount + campaign.OpposeCount;
            var approvalPercent = totalVotes > 0
                ? (int)(100 * (double)campaign.ApproveCount / totalVotes)
                : 0;

            var color = approvalPercent switch
            {
                >= 60 => Color.Green,
                >= 40 => Color.Gold,
                _ => Color.Red,
            };

            var expectedCloseTime = campaign.GetExpectedCampaignCloseTimeStamp();

            var timeRemainingLabel = campaign.Outcome switch
            {
                PromotionCampaignOutcome.Accepted => "The campaign was accepted.",
                PromotionCampaignOutcome.Rejected => "The campaign was rejected.",
                null => $"The campaign can be closed <t:{expectedCloseTime.ToUnixTimeSeconds()}:R>.",
                _ => "An error occurred while closing the campaign.",
            };

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Promotion campaign {campaign.Id}")
                .WithUrl(url)
                .WithColor(color)
                .WithDescription($"""
                    {MentionUtils.MentionUser(campaign.Subject.Id)} {(campaign.Outcome is null ? "is" : "was")} up for promotion to {MentionUtils.MentionRole(campaign.TargetRole.Id)}

                    👍 {campaign.ApproveCount} / 👎 {campaign.OpposeCount} ({approvalPercent}% approval)

                    {timeRemainingLabel}
                    """);

            return embedBuilder.Build();
        }

        [return: NotNullIfNotNull(nameof(campaign))]
        private static MessageComponent? GetCampaignComponents(PromotionCampaignSummary? campaign)
        {
            if (campaign is null)
                return null;

            return new ComponentBuilder()
                .WithButton("Approve", style: ButtonStyle.Success, customId: $"button_campaign_approve:{campaign.Id}")
                .WithButton("Oppose", style: ButtonStyle.Danger, customId: $"button_campaign_oppose:{campaign.Id}")
                .WithButton("Change comment", style: ButtonStyle.Primary, customId: $"button_campaign_comment:{campaign.Id}")
                .WithButton("Refresh", style: ButtonStyle.Secondary, customId: $"button_campaign_refresh:{campaign.Id}")
                .Build();
        }
    }

    public class PromotionCommentModal : IModal
    {
        public required string Title { get; set; }

        [ModalTextInput("campaign_comment", TextInputStyle.Paragraph)]
        [RequiredInput(false)]
        public string? Content { get; set; }
    }
}
