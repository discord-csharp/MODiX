using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Promotions;
using Modix.Services.Promotions;

namespace Modix.Modules
{
    [Name("Promotions")]
    [Summary("Manage promotion campaigns")]
    [Group("promotions")]
    public class PromotionsModule : ModuleBase
    {
        private const string DefaultApprovalMessage = "I approve of this nomination.";

        public PromotionsModule(IPromotionsService promotionsService)
        {
            PromotionsService = promotionsService;
        }

        [Command("campaigns"), Alias("")]
        [Summary("List all active promotion campaigns")]
        public async Task Campaigns()
        {
            var campaigns = await PromotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria()
            {
                GuildId = Context.Guild.Id,
                IsClosed = false                
            });

            var embed = new EmbedBuilder()
            {
                Title = Format.Bold("Active Promotion Campaigns"),
                Url = "https://mod.gg/promotions",
                Color = Color.Gold,
                Timestamp = DateTimeOffset.Now,
                Description = campaigns.Any() ? null : "There are no active promotion campaigns."
            };

            foreach (var campaign in campaigns)
            {
                var totalVotes = campaign.CommentCounts
                    .Select(x => x.Value)
                    .Sum();
                var totalApprovals = campaign.CommentCounts
                    .Where(x => x.Key == PromotionSentiment.Approve)
                    .Select(x => x.Value)
                    .Sum();
                var approvalPercentage = Math.Round((float)totalApprovals / totalVotes * 100);

                var idLabel = $"#{campaign.Id}";
                var votesLabel = (totalVotes == 1) ? "Vote" : "Votes";
                var approvalLabel = Format.Italics($"{approvalPercentage}% approval");

                embed.AddField(new EmbedFieldBuilder()
                {
                    Name = $"{Format.Bold(idLabel)}: For {Format.Bold(campaign.Subject.DisplayName)} to {Format.Bold(campaign.TargetRole.Name)}",
                    Value = $"{totalVotes} {votesLabel} ({approvalLabel})",
                    IsInline = false
                });
            }

            await ReplyAsync("", embed: embed);
        }

        [Command("nominate")]
        [Summary("Nominate the given user for promotion")]
        public async Task Nominate(
            [Summary("The user to nominate")]
                IGuildUser subject,
            [Remainder]
            [Summary("A comment to be attached to the new campaign")]
                string comment)
        {
            var continuationData = await PromotionsService.CreateCampaignAsync(subject.Id, comment);

            var confirmationMessage = await ReplyAsync($@"You are nominating user {subject.Id} for promotion to rank {continuationData.TargetRankRole.Name}.
React with ✅ in the next 10 seconds to finalize creation of the campaign.");

            await confirmationMessage.AddReactionAsync(new Emoji("✅"));

            await Task.Delay(10000);

            var reactionUsers = await confirmationMessage.GetReactionUsersAsync("✅");

            if (reactionUsers.Any(u => u.Id == continuationData.NominatingUserId))
            {
                await PromotionsService.ContinueCreateCampaignAsync(continuationData);
            }
            else
            {
                await confirmationMessage.RemoveAllReactionsAsync();
                await confirmationMessage.AddReactionAsync(new Emoji("❌"));
            }
        }

        [Command("comment")]
        [Summary("Comment on an ongoing campaign to promote a user.")]
        public Task Comment(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Summary("The sentiment of the comment")]
                PromotionSentiment sentiment,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
            => PromotionsService.AddCommentAsync(campaignId, sentiment, content);

        [Command("approve")]
        [Summary("Alias to approve on an ongoing campaign to promote a user.")]
        public Task Approve(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
            => PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Approve, content);

        [Command("approve")]
        [Summary("Alias to approve on an ongoing campaign to promote a user.")]
        public Task Approve(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId)
            => PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Approve, DefaultApprovalMessage);

        [Command("oppose")]
        [Summary("Alias to oppose on an ongoing campaign to promote a user.")]
        public Task Oppose(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
            => PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Oppose, content);

        [Command("abstain")]
        [Summary("Alias to abstain from an ongoing campaign to promote a user.")]
        public Task Abstain(
            [Summary("The ID value of the campaign to be commented upon")]
                long campaignId,
            [Remainder]
            [Summary("The content of the comment")]
                string content)
            => PromotionsService.AddCommentAsync(campaignId, PromotionSentiment.Abstain, content);

        [Command("accept")]
        [Summary("Accept an ongoing campaign to promote a user, and perform the promotion.")]
        public Task Accept(
            [Summary("The ID value of the campaign to be accepted.")]
                long campaignId)
            => PromotionsService.AcceptCampaignAsync(campaignId);

        [Command("reject")]
        [Summary("Reject an ongoing campaign to promote a user.")]
        public Task Reject(
            [Summary("The ID value of the campaign to be rejected.")]
                long campaignId)
            => PromotionsService.RejectCampaignAsync(campaignId);

        internal protected IPromotionsService PromotionsService { get; }
    }
}
