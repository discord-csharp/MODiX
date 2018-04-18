using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Utilities;

namespace Modix.Services.Promotions
{
    public class PromotionService
    {
        private Regex _badCharacterRegex = new Regex(@"[\u200B-\u200D\uFEFF]");

#if DEBUG
        //TODO: Un-hardcode this
        private const ulong _regularRoleId = 385925278364598302;
        private const ulong _staffRoleId = 268470383571632128;
        private const ulong _promotionChannelId = 436251483017838594;
#else
        private const ulong _regularRoleId = 246266977553874944;
        private const ulong _staffRoleId = 268470383571632128;
        private const ulong _promotionChannelId = 411991461832294400;
#endif
        private DiscordSocketClient _client;
        private IPromotionRepository _repository;

        private SocketGuild CurrentGuild => _client.Guilds.First();
        private IMessageChannel PromotionChannel => CurrentGuild.GetChannel(_promotionChannelId) as IMessageChannel;

        public PromotionService(DiscordSocketClient client, IPromotionRepository repository)
        {
            _client = client;
            _repository = repository;
        }

        public Task<IEnumerable<PromotionCampaign>> GetCampaigns() => _repository.GetCampaigns();
        public Task<PromotionCampaign> GetCampaign(int id) => _repository.GetCampaign(id);

        public async Task ApproveCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            ThrowIfNotStaff(promoter);

            var foundUser = CurrentGuild.GetUser(campaign.UserId);
            var foundRole = CurrentGuild.Roles.FirstOrDefault(d => d.Id == _regularRoleId);

            if (foundRole == null)
            {
                throw new InvalidOperationException("The server does not have a 'Regular' role to grant.");
            }

            await foundUser.AddRoleAsync(foundRole);

            campaign.Status = CampaignStatus.Approved;
            await _repository.UpdateCampaign(campaign);

            await PromotionChannel?.SendMessageAsync($"{MentionUtils.MentionUser(campaign.UserId)} has been promoted to Regular! 🎉");
        }

        public async Task DenyCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            ThrowIfNotStaff(promoter);

            if (campaign.Status == CampaignStatus.Denied)
            {
                throw new InvalidOperationException("The campaign has already been denied.");
            }

            campaign.Status = CampaignStatus.Denied;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task ActivateCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            ThrowIfNotStaff(promoter);

            if (campaign.Status != CampaignStatus.Denied)
            {
                throw new InvalidOperationException("Cannot reactivate a campaign that has not been denied.");
            }

            campaign.Status = CampaignStatus.Active;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task AddComment(PromotionCampaign campaign, string comment, PromotionSentiment sentiment)
        {
            comment = _badCharacterRegex.Replace(comment, "");

            if (comment.Trim().Length < 10)
            {
                throw new ArgumentException("Comment is too short, must be more than 10 characters.");
            }

            if (comment.Length > 1000)
            {
                throw new ArgumentException("Comment is too long, must be under 1000 characters.");
            }

            if (campaign.Status != CampaignStatus.Active)
            {
                throw new ArgumentException("Campaign must be active to comment.");
            }

            var promotionComment = new PromotionComment
            {
                PostedDate = DateTimeOffset.UtcNow,
                Body = comment,
                Sentiment = sentiment
            };

            await _repository.AddCommentToCampaign(campaign, promotionComment);
        }

        public void ThrowIfNotStaff(SocketGuildUser user)
        {
            if (CurrentGuild.Owner == user)
            {
                return;
            }

            if (!user.HasRole(_staffRoleId))
            {
                throw new ArgumentException("The given promoter is not a staff member.");
            }
        }

        public async Task<PromotionCampaign> CreateCampaign(SocketGuildUser user, string commentBody)
        {
            if ((await GetCampaigns()).Any(d=>d.UserId == user.Id))
            {
                throw new ArgumentException("A campaign already exists for that user.");
            }

            if (user.Roles.Count > 1)
            {
                throw new ArgumentException("Recommended user must be unranked.");
            }

            var timeOnServer = DateTimeOffset.UtcNow - user.JoinedAt.Value.ToUniversalTime();

            if (timeOnServer < TimeSpan.FromDays(30))
            {
                throw new ArgumentException($"Recommended user must have been a member of the server for more than 30 days. Currently: {timeOnServer.TotalDays}");
            }

            var ret = new PromotionCampaign
            {
                UserId = user.Id,
                Username = $"{user.Username}#{user.Discriminator}",
                StartDate = DateTimeOffset.UtcNow,
                Status = CampaignStatus.Active
            };

            await AddComment(ret, commentBody, PromotionSentiment.For);

            await _repository.AddCampaign(ret);

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
