using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Modix.Bot.Extensions;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Extensions;
using Modix.Services.Images;
using Modix.Services.Promotions;
using Modix.Services.Utilities;


namespace Modix.Bot.Behaviors
{
    public class PromotionDialogBehavior :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<PromotionActionCreatedNotification>
    {
        private readonly TimeSpan ErrorRemoveDelay = TimeSpan.FromSeconds(5);

        private readonly DateTime _utcNow = DateTime.UtcNow;

        private static readonly Emoji _approveEmoji = new Emoji("👍");

        private static readonly Emoji _disapproveEmoji = new Emoji("👎");

        private IDesignatedChannelService _designatedChannelService { get; }

        private IDiscordSocketClient _discordSocketClient { get; }

        private IMemoryCache _memoryCache { get; }

        private IPromotionsService _promotionsService { get; }

        private IImageService _imageService { get; }

        private IPromotionCampaignRepository _promotionCampaignRepository { get; }

        private IAuthorizationService _authorizationService { get; }

        public PromotionDialogBehavior(IDesignatedChannelService designatedChannelService,
            IDiscordSocketClient discordSocketClient,
            IMemoryCache memoryCache,
            IPromotionsService promotionsService,
            IImageService imageService,
            IAuthorizationService authorizationService,
            IPromotionCampaignRepository promotionCampaignRepository)
        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _memoryCache = memoryCache;
            _promotionsService = promotionsService;
            _imageService = imageService;
            _promotionCampaignRepository = promotionCampaignRepository;
            _authorizationService = authorizationService;
        }

        public async Task HandleNotificationAsync(PromotionActionCreatedNotification notification,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            switch (notification.Data.Type)
            {
                case PromotionActionType.CampaignCreated:
                    await CreatePromoDialog(notification.Data);
                    break;
                case PromotionActionType.CampaignClosed:
                    await DeletePromoDialog(notification.Data);
                    break;
            }
        }

        public async Task HandleNotificationAsync(ReactionAddedNotification notification,
            CancellationToken cancellationToken = default)
        {
            var reaction = notification.Reaction.Emote;
            var user = (IGuildUser)notification.Reaction.User.Value;
            var message = await notification.Message.GetOrDownloadAsync();

            if (cancellationToken.IsCancellationRequested
                || notification.Reaction.User.Value.IsBot
                || message.Embeds.Count < 1)
                return;

            if (_memoryCache.TryGetValue(message.Id, out CachedPromoDialog poll)
            && ValidateVoteReaction(reaction))
            {
                string voteError = null;
                var sentiment = reaction.Name == _approveEmoji.Name ? PromotionSentiment.Approve : PromotionSentiment.Oppose;

                await message.RemoveReactionAsync(notification.Reaction.Emote, notification.Reaction.User.Value);

                try
                {
                    await _authorizationService.OnAuthenticatedAsync(user);
                    await _promotionsService.UpdateOrAddComment(poll.CampaignId, sentiment, "This vote was cast from Discord");
                }
                catch (Exception e)
                {
                    voteError = e.Message;
                }

                var campaign = await _promotionCampaignRepository.GetCampignSummaryByIdAsync(poll.CampaignId);

                if(!campaign.CommentCounts.TryGetValue(PromotionSentiment.Approve, out var approveCount))
                    approveCount = 0;
                if(!campaign.CommentCounts.TryGetValue(PromotionSentiment.Oppose, out var opposeCount))
                    opposeCount = 0;

                await message.ModifyAsync(m =>
                    m.Embed = BuildPollEmbed(campaign, approveCount, opposeCount, voteError).Build());

                await Task.Delay(ErrorRemoveDelay, cancellationToken);

                if(voteError != null)
                    await message.ModifyAsync(m =>
                        m.Embed = BuildPollEmbed(campaign, approveCount, opposeCount).Build());
            }
        }

        private async Task CreatePromoDialog(PromotionActionCreationData campaign)
        {
            if (!await _designatedChannelService.AnyDesignatedChannelAsync(campaign.GuildId,
                DesignatedChannelType.PromotionInterface))
                return;

            var pollChannel = await GetPollChannel(_discordSocketClient.GetGuild(campaign.GuildId));

            var campaignSummary = await _promotionCampaignRepository.GetCampignSummaryByIdAsync(campaign.Campaign.Id);

            var message = await pollChannel.SendMessageAsync(embed: BuildPollEmbed(campaignSummary, 1, 0).Build());

            await message.AddReactionsAsync(new[] {(IEmote)_approveEmoji, _disapproveEmoji});

            var poll = new CachedPromoDialog
            {
                MessageId = message.Id,
                CampaignId = campaignSummary.Id,
            };

            SetDialogCache(campaign, message, poll);
        }

        private async Task DeletePromoDialog(PromotionActionCreationData campaign)
        {
            if (!_memoryCache.TryGetValue(campaign.Campaign.Id, out CachedPromoDialog poll))
                return;
            var pollChannel = await GetPollChannel(_discordSocketClient.GetGuild(campaign.GuildId));
            var message =  await pollChannel.GetMessageAsync(poll.MessageId);
            await message.DeleteAsync();

        }

        private EmbedBuilder BuildPollEmbed(PromotionCampaignSummary campaign, int Approve, int Oppose, string error = null)
        {
                var boldRole = $"**{MentionUtils.MentionRole(campaign.TargetRole.Id)}**";
                var user = _discordSocketClient.GetUser(campaign.Subject.Id);

                ValueTask<Color> aviColor;

                if ((user.GetAvatarUrl(size: 16) ?? user.GetDefaultAvatarUrl()) is { } avatarUrl)
                {
                    aviColor = _imageService.GetDominantColorAsync(new Uri(avatarUrl));
                }
                else
                {
                    aviColor = new ValueTask<Color>(Color.Teal);
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"#{campaign.Id}")
                    .WithDescription($"**{campaign.Subject.Nickname}** Has been nominated for promotion to {boldRole} ")
                    .WithColor(aviColor.Result)
                    .WithUserAsAuthor(user)
                    .AddField("The current vote total stands at: ", $" {Approve} {_approveEmoji} / {Oppose} {_disapproveEmoji}")
                    .WithTimestamp(_utcNow)
                    .WithFooter("Please react with the appropriate reaction to cast your vote!");

                return error == null ? embed : embed.AddField("Vote Error: ", error);
        }

        private async Task<ITextChannel> GetPollChannel(IGuild guild)
        {
            var getPollChannel = await _designatedChannelService
                .GetDesignatedChannelsAsync(guild, DesignatedChannelType.PromotionInterface);
            return getPollChannel.First() as ITextChannel;
        }

        private void SetDialogCache(PromotionActionCreationData campaign, IUserMessage message, CachedPromoDialog dialog)
        {
            _memoryCache.Set(campaign.Campaign.Id, dialog);
            _memoryCache.Set(message.Id, dialog);
        }

        private static bool ValidateVoteReaction(IEmote emote)
            => emote.Name == _approveEmoji.Name || emote.Name == _disapproveEmoji.Name;
    }
}
