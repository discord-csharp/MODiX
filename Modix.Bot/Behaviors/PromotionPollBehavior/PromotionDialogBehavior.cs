using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Images;
using Modix.Services.Promotions;
using Modix.Services.Utilities;


namespace Modix.Bot.Behaviors
{
    public class PromotionDialogBehavior :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<PromotionActionCreatedNotification>
    {
        private static readonly TimeSpan ErrorRemoveDelay = TimeSpan.FromSeconds(5);

        private readonly DateTime _utcNow = DateTime.UtcNow;

        private static readonly Emoji _approveEmoji = new Emoji("👍");

        private static readonly Emoji _disapproveEmoji = new Emoji("👎");

        private readonly IDesignatedChannelService _designatedChannelService;

        private readonly IDiscordSocketClient _discordSocketClient;

        private readonly IMemoryCache _memoryCache;

        private readonly IPromotionsService _promotionsService;

        private readonly IImageService _imageService;

        private readonly IPromotionCampaignRepository _promotionCampaignRepository;

        private readonly IAuthorizationService _authorizationService;

        private readonly IPromotionDialogRepository _promotionDialogRepository;

        public PromotionDialogBehavior(
            IDesignatedChannelService designatedChannelService,
            IDiscordSocketClient discordSocketClient,
            IMemoryCache memoryCache,
            IPromotionsService promotionsService,
            IImageService imageService,
            IAuthorizationService authorizationService,
            IPromotionCampaignRepository promotionCampaignRepository,
            IPromotionDialogRepository promotionDialogRepository
            )
        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _memoryCache = memoryCache;
            _promotionsService = promotionsService;
            _imageService = imageService;
            _promotionCampaignRepository = promotionCampaignRepository;
            _authorizationService = authorizationService;
            _promotionDialogRepository = promotionDialogRepository;
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

            if (notification.Reaction.User.Value.IsBot || message.Embeds.Count < 1)
                return;

            if (_memoryCache.TryGetValue(message.Id, out CachedPromoDialog poll)
            && IsRelevantReaction(reaction))
            {
                string voteError = null;
                var sentiment = reaction.Name == _approveEmoji.Name ? PromotionSentiment.Approve : PromotionSentiment.Oppose;

                await message.RemoveReactionAsync(notification.Reaction.Emote, notification.Reaction.User.Value);

                try
                {
                    await _authorizationService.OnAuthenticatedAsync(user);
                    await _promotionsService.UpdateOrAddCommentAsync(poll.CampaignId, sentiment, "This vote was cast from Discord");
                }
                catch (Exception e)
                {
                    voteError = e.Message;
                }

                var campaign = await _promotionCampaignRepository.GetCampaignSummaryByIdAsync(poll.CampaignId);
                await message.ModifyAsync(async m =>
                    m.Embed = (await BuildPollEmbed(campaign, campaign.ApproveCount, campaign.OpposeCount, voteError)).Build());

                if(voteError != null)
                {
                    await Task.Delay(ErrorRemoveDelay, cancellationToken);
                    await message.ModifyAsync(async m =>
                        m.Embed = (await BuildPollEmbed(campaign, campaign.ApproveCount, campaign.OpposeCount)).Build());
                }
            }
        }

        private async Task CreatePromoDialog(PromotionActionCreationData campaign)
        {
            if (!await _designatedChannelService.AnyDesignatedChannelAsync(campaign.GuildId,
                DesignatedChannelType.PromotionInterface))
                return;

            var pollChannel = await GetPollChannel(_discordSocketClient.GetGuild(campaign.GuildId));

            var campaignSummary = await _promotionCampaignRepository.GetCampaignSummaryByIdAsync(campaign.Campaign.Id);

            var message = await pollChannel.SendMessageAsync(embed: (await BuildPollEmbed(campaignSummary, 1, 0)).Build());

            await message.AddReactionsAsync(new[] {(IEmote)_approveEmoji, _disapproveEmoji});

            SetDialogCache( new CachedPromoDialog
            {
                MessageId = message.Id,
                CampaignId = campaignSummary.Id,
            });

            await _promotionDialogRepository.CreateAsync(message.Id, campaign.Campaign.Id);
        }

        private async Task DeletePromoDialog(PromotionActionCreationData campaign)
        {
            if (!_memoryCache.TryGetValue(GetKey(campaign.Campaign.Id), out CachedPromoDialog poll))
                return;
            var pollChannel = await GetPollChannel(_discordSocketClient.GetGuild(campaign.GuildId));

            var message =  await pollChannel.GetMessageAsync(poll.MessageId);

            if (!await _promotionDialogRepository.TryDeleteAsync(message.Id))
                throw new InvalidOperationException("Dialog to be deleted does not exist");

            _memoryCache.Remove(GetKey(poll.CampaignId));
            _memoryCache.Remove(poll.MessageId);
            await message.DeleteAsync();
        }

        private async Task<EmbedBuilder> BuildPollEmbed(PromotionCampaignSummary campaign, int Approve, int Oppose, string error = null)
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
                    .WithDescription($"**{campaign.Subject.Nickname}** has been nominated for promotion to {boldRole} ")
                    .WithColor(await aviColor)
                    .WithUserAsAuthor(user)
                    .AddField("The current vote total stands at: ", $" {Approve} {_approveEmoji} / {Oppose} {_disapproveEmoji}")
                    .WithTimestamp(_utcNow)
                    .WithFooter("Please react with the appropriate reaction to cast your vote!");

                return error == null ? embed : embed.AddField("⚠️ Vote Error: ⚠️", error);
        }

        private async Task<ITextChannel> GetPollChannel(IGuild guild)
        {
            var getPollChannel = await _designatedChannelService
                .GetDesignatedChannelsAsync(guild, DesignatedChannelType.PromotionInterface);
            return getPollChannel.First() as ITextChannel;
        }

        private void SetDialogCache(CachedPromoDialog dialog)
        {
            _memoryCache.Set(GetKey(dialog.CampaignId), dialog);
            _memoryCache.Set(dialog.MessageId, dialog);
        }

        private static object GetKey(long id) => new { Target = "PromotionDialogBehavior", id };

        private static bool IsRelevantReaction(IEmote emote)
            => emote.Name == _approveEmoji.Name || emote.Name == _disapproveEmoji.Name;
    }
}
