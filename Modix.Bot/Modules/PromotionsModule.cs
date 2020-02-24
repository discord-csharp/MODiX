using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Options;

using Modix.Bot.Extensions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Utilities;
using Modix.Services.CommandHelp;
using Modix.Services.Promotions;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Promotions")]
    [Summary("Manage promotion campaigns.")]
    [Group("promotions")]
    [Alias("promotion", "campaign", "campaigns", "promo")]
    [HelpTags("campaigns", "nominate", "nomination", "nominating")]
    public class PromotionsModule : ModuleBase
    {
        private const string DefaultApprovalMessage = "I approve of this nomination.";

        public PromotionsModule(IPromotionsService promotionsService, IOptions<ModixConfig> config)
        {
            PromotionsService = promotionsService;
            Config = config.Value;
        }

        [Command("campaigns")]
        [Alias("", "list")]
        [Summary("List all active promotion campaigns")]
        public async Task CampaignsAsync()
        {
            var campaigns = await PromotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria()
            {
                GuildId = Context.Guild.Id,
                IsClosed = false
            });

            // https://mod.gg/promotions
            var url = new UriBuilder(Config.WebsiteBaseUrl)
            {
                Path = "/promotions"
            }.RemoveDefaultPort().ToString();

            var embed = new EmbedBuilder()
            {
                Title = Format.Bold("Active Promotion Campaigns"),
                Url = url,
                Color = Color.Gold,
                Timestamp = DateTimeOffset.Now,
                Description = campaigns.Any() ? null : "There are no active promotion campaigns."
            };

            foreach (var campaign in campaigns)
            {
                var idLabel = $"#{campaign.Id}";

                var approvalLabel = $"👍 {campaign.ApproveCount} / 👎 {campaign.OpposeCount}";
                var timeRemaining = campaign.GetTimeUntilCampaignCanBeClosed();
                var timeRemainingLabel = timeRemaining < TimeSpan.FromSeconds(1) ? "Can be closed now" : $"{timeRemaining.Humanize(precision: 2, minUnit: TimeUnit.Minute)} until close";

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"{Format.Bold(idLabel)}: {Format.Bold(campaign.Subject.GetFullUsername())} to {campaign.TargetRole.Name}",
                    Value = $"{approvalLabel} ({timeRemainingLabel})",
                    IsInline = false
                });
            }

            await ReplyAsync("", embed: embed.Build());
        }

        [Command("nominate")]
        [Summary("Nominate the given user for promotion")]
        public async Task NominateAsync(
            [Summary("The user to nominate")]
                IGuildUser subject,
            [Remainder]
            [Summary("A comment to be attached to the new campaign")]
                string comment)
            => await PromotionsService.CreateCampaignAsync(subject.Id, comment,
                c => Context.GetUserConfirmationAsync(
                    $"You are nominating {subject.GetFullUsername()} ({subject.Id}) for promotion to {c.TargetRankRole.Name}.{Environment.NewLine}"));

        [Command("comment")]
        [Summary("Comment on an ongoing campaign to promote a user.")]
        public async Task CommentAsync(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Summary("The sentiment of the comment")]
                PromotionSentiment sentiment,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
        {
            await PromotionsService.AddCommentAsync(campaignId, sentiment, content);
            await Context.AddConfirmation();
        }

        [Command("approve")]
        [Summary("Alias to approve on an ongoing campaign to promote a user.")]
        public async Task ApproveAsync(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content = DefaultApprovalMessage)
        {
            await PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Approve, content);
            await Context.AddConfirmation();
        }

        [Command("oppose")]
        [Summary("Alias to oppose on an ongoing campaign to promote a user.")]
        public async Task OpposeAsync(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
        {
            await PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Oppose, content);
            await Context.AddConfirmation();
        }

        [Command("abstain")]
        [Summary("Alias to abstain from an ongoing campaign to promote a user.")]
        public async Task AbstainAsync(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
        {
            await PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Abstain, content);
            await Context.AddConfirmation();
        }

        [Command("accept")]
        [Summary("Accept an ongoing campaign to promote a user, and perform the promotion.")]
        public async Task AcceptAsync(
            [Summary("The ID value of the campaign to be accepted.")]
                long campaignId,
            [Summary("Whether to bypass the time restriction on campaign acceptance")]
                bool force = false)
        {
            await PromotionsService.AcceptCampaignAsync(campaignId, force);
            await Context.AddConfirmation();
        }

        [Command("reject")]
        [Summary("Reject an ongoing campaign to promote a user.")]
        public async Task RejectAsync(
            [Summary("The ID value of the campaign to be rejected.")]
                long campaignId)
        {
            await PromotionsService.RejectCampaignAsync(campaignId);
            await Context.AddConfirmation();
        }

        internal protected IPromotionsService PromotionsService { get; }

        internal protected ModixConfig Config { get; }
    }
}
