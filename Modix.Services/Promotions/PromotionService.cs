using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Utilities;

namespace Modix.Services.Promotions
{
    public class PromotionService
    {
        //TODO: Un-hardcode this
        private const ulong _regularRoleId = 246266977553874944;
        private const ulong _staffRoleId = 268470383571632128;

        private DiscordSocketClient _client;
        private IPromotionRepository _repository;

        public PromotionService(DiscordSocketClient client, IPromotionRepository repository)
        {
            _client = client;
            _repository = repository;
        }

        public Task<IEnumerable<PromotionCampaign>> GetCampaigns() => _repository.GetCampaigns();
        public Task<PromotionCampaign> GetCampaign(int id) => _repository.GetCampaign(id);

        public async Task ApproveCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            if (!promoter.HasRole(_staffRoleId))
            {
                throw new ArgumentException("The given promoter is not a staff member.");
            }

            var foundUser = _client.Guilds.First().GetUser(campaign.UserId);
            var foundRole = _client.Guilds.First().Roles.FirstOrDefault(d => d.Id == _regularRoleId);

            if (foundRole == null)
            {
                throw new InvalidOperationException("The server does not have a 'Regular' role to grant.");
            }

            await foundUser.AddRoleAsync(foundRole);

            campaign.Status = CampaignStatus.Approved;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task DenyCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            if (!promoter.HasRole(_staffRoleId))
            {
                throw new ArgumentException("The given promoter is not a staff member.");
            }

            campaign.Status = CampaignStatus.Denied;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task ActivateCampaign(SocketGuildUser promoter, PromotionCampaign campaign)
        {
            if (!promoter.HasRole(_staffRoleId))
            {
                throw new ArgumentException("The given promoter is not a staff member.");
            }

            campaign.Status = CampaignStatus.Active;
            await _repository.UpdateCampaign(campaign);
        }

        public async Task AddComment(PromotionCampaign campaign, SocketGuildUser commentor, string comment, PromotionSentiment sentiment)
        {
            if (comment.Length < 10)
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
                Sentiment = sentiment,
                Id = (campaign.Id.GetHashCode() / 2) + (commentor.Id.GetHashCode() / 2)
            };

            await _repository.AddCommentToCampaign(campaign, promotionComment);
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
                Username = user.Nickname ?? user.Username,
                StartDate = DateTimeOffset.UtcNow,
                Id = user.Id.GetHashCode(),
                Status = CampaignStatus.Active
            };

            await AddComment(ret, user, commentBody, PromotionSentiment.For);

            await _repository.AddCampaign(ret);

            return ret;
        }
    }
}
