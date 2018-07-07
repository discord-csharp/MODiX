using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotion;
using Modix.Services.Utilities;

namespace Modix.Services.Promotions
{
    public class PromotionService
    {
        private readonly Regex _badCharacterRegex = new Regex(@"[\u200B-\u200D\uFEFF]");

        private readonly DiscordSocketClient _client;
        private readonly IPromotionRepository _repository;

        private readonly ulong allowedToCommentRoleID;
        private readonly ulong allowedToCreateRoleID;
        private readonly ulong promotionChannelID;

        public PromotionService(DiscordSocketClient client, IPromotionRepository repository, ModixConfig config)
        {
            _client = client;
            _repository = repository;
            promotionChannelID = config.ChannelIdForPromotionCampaignAnnouncement;
            allowedToCommentRoleID = config.RoleIdToAllowCommentingOnPromotionCampaign;
            allowedToCreateRoleID = config.RoleIdToAllowCreatingPromotionCampaign;
        }

        private SocketGuild CurrentGuild => _client.Guilds.First();
        private IMessageChannel PromotionChannel => CurrentGuild.GetChannel(promotionChannelID) as IMessageChannel;

        public Task<IEnumerable<PromotionCampaignEntity>> GetCampaigns()
        {
            return _repository.GetCampaigns();
        }

        public Task<PromotionCampaignEntity> GetCampaign(int id)
        {
            return _repository.GetCampaign(id);
        }

        public async Task ApproveCampaign(SocketGuildUser promoter, PromotionCampaignEntity campaign)
        {
            ThrowIfNotStaff(promoter);

            var foundUser = CurrentGuild.GetUser(campaign.PromotionFor.DiscordUserId);
            var foundRole = CurrentGuild.Roles.FirstOrDefault(d => d.Id == allowedToCommentRoleID);

            if (foundRole == null)
                throw new InvalidOperationException("The server does not have a 'Regular' role to grant.");

            await foundUser.AddRoleAsync(foundRole);

            campaign.Status = CampaignStatus.Approved;
            await _repository.UpdateCampaign(campaign);

            if (PromotionChannel == null)
                throw new NullReferenceException(nameof(PromotionChannel));
            await PromotionChannel?.SendMessageAsync(
                $"{MentionUtils.MentionUser(campaign.PromotionFor.DiscordUserId)} has been promoted to Regular! 🎉");
        }

        public async Task DenyCampaign(SocketGuildUser promoter, PromotionCampaignEntity campaign)
        {
            ThrowIfNotStaff(promoter);

            if (campaign.Status == CampaignStatus.Denied)
                throw new InvalidOperationException("The campaign has already been denied.");

            campaign.Status = CampaignStatus.Denied;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task ActivateCampaign(SocketGuildUser promoter, PromotionCampaignEntity campaign)
        {
            ThrowIfNotStaff(promoter);

            if (campaign.Status != CampaignStatus.Denied)
                throw new InvalidOperationException("Cannot reactivate a campaign that has not been denied.");

            campaign.Status = CampaignStatus.Active;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task AddComment(PromotionCampaignEntity campaign, string comment, PromotionSentiment sentiment)
        {
            comment = _badCharacterRegex.Replace(comment, "");

            if (comment.Trim().Length < 10)
                throw new ArgumentException("Comment is too short, must be more than 10 characters.");

            if (comment.Length > 1000)
                throw new ArgumentException("Comment is too long, must be under 1000 characters.");

            if (campaign.Status != CampaignStatus.Active)
                throw new ArgumentException("Campaign must be active to comment.");

            var promotionComment = new PromotionCommentEntity
            {
                PostedDate = DateTimeOffset.UtcNow,
                Body = comment,
                Sentiment = sentiment
            };

            await _repository.AddCommentToCampaign(campaign, promotionComment);
        }

        public void ThrowIfNotStaff(SocketGuildUser user)
        {
            if (CurrentGuild.Owner == user) return;

            if (!user.HasRole(allowedToCreateRoleID))
                throw new ArgumentException("The given promoter is not a staff member.");
        }

        public async Task<PromotionCampaignEntity> CreateCampaign(SocketGuildUser user, string commentBody)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(commentBody))
                throw new ArgumentException("Comment cannot be empty!");
            if ((await GetCampaigns()).Any(d => d.PromotionFor.DiscordUserId == user.Id))
                throw new ArgumentException("A campaign already exists for that user.");

            if (user.Roles.Count > 1) throw new ArgumentException("Recommended user must be unranked.");

            var timeOnServer = DateTimeOffset.UtcNow - user.JoinedAt.Value.ToUniversalTime();

            if (timeOnServer < TimeSpan.FromDays(30))
                throw new ArgumentException(
                    $"Recommended user must have been a member of the server for more than 30 days. Currently: {timeOnServer.TotalDays}");

            var ret = new PromotionCampaignEntity
            {
                StartDate = DateTimeOffset.UtcNow,
                Status = CampaignStatus.Active
            };

            await _repository.AddCampaign(ret, user);

            await AddComment(ret, commentBody, PromotionSentiment.For);

            if (PromotionChannel == null)
                throw new NullReferenceException(nameof(promotionChannelID));

            await PromotionChannel?.SendMessageAsync("", false,
                new EmbedBuilder()
                    .WithTitle("Campaign Started")
                    .WithAuthor(user)
                    .WithDescription(commentBody)
                    .WithFooter("Vote now at https://mod.gg/promotions"));

            return ret;
        }
    }
}