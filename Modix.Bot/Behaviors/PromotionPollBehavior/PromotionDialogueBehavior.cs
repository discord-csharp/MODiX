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
using Modix.Services.Core;
using Modix.Services.Images;
using Modix.Services.Promotions;
using Modix.Services.Utilities;

namespace Modix.Bot.Behaviors
{
    public class PromotionDialogueBehavior :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<PromotionActionCreatedNotification>
    {
        private readonly DateTime _utcNow = DateTime.UtcNow;

        private static readonly Emoji _approveEmoji = new Emoji("👍");

        private static readonly Emoji _disproveEmoji = new Emoji("👎");

        private IDesignatedChannelService _designatedChannelService { get; }

        private IDiscordSocketClient _discordSocketClient { get; }

        private IMemoryCache _memoryCache { get; }

        private IPromotionsService _promotionsService { get; }

        private IImageService _imageService { get; }

        public PromotionDialogueBehavior(IDesignatedChannelService designatedChannelService,
            IDiscordSocketClient discordSocketClient,
            IMemoryCache memoryCache,
            IPromotionsService promotionsService,
            IImageService imageService)

        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _memoryCache = memoryCache;
            _promotionsService = promotionsService;
            _imageService = imageService;
        }

        public async Task HandleNotificationAsync(PromotionActionCreatedNotification notification,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            switch (notification.Data.Type)
            {
                case PromotionActionType.CampaignCreated:
                    await CreatePromoPoll(notification.Data);
                    break;
                case PromotionActionType.CampaignClosed:
                    await DeletePromoPoll(notification.Data);
                    break;
            }
        }

        public async Task HandleNotificationAsync(ReactionAddedNotification notification,
            CancellationToken cancellationToken = default)
        {
            var reaction = notification.Reaction.Emote;
            var user = (IGuildUser)notification.Reaction.User.Value;

            if (cancellationToken.IsCancellationRequested
                || notification.Reaction.User.Value.IsBot
                || notification.Message.Value.Embeds.Count < 1)
                return;

            if (_memoryCache.TryGetValue(notification.Message.Id, out CachedPromoPoll poll)
            && ValidateVoteReaction(reaction))
            {
                await notification.Message.Value.RemoveReactionAsync(notification.Reaction.Emote, notification.Reaction.User.Value);

                var sentiment = reaction.Name == _approveEmoji.Name ? PromotionSentiment.Approve : PromotionSentiment.Oppose;

                if (!await _promotionsService.VotePromoAsync(poll.Campaign.Id, user,  sentiment))
                    return;

                if (reaction.Name == _approveEmoji.Name)
                    ++poll.Approve;
                else if(reaction.Name == _disproveEmoji.Name)
                    ++poll.Disprove;

                await poll.Message.ModifyAsync(m => m.Embed = BuildPollEmbed(poll.Campaign, poll.Approve, poll.Disprove).Build());
            }
        }

        private async Task CreatePromoPoll(PromotionActionCreationData campaign)
        {
            var pollChannel = await GetPollChannel(_discordSocketClient.GetGuild(campaign.GuildId));
            var message = await pollChannel.SendMessageAsync(embed: BuildPollEmbed(campaign.Campaign, 1, 0).Build());

            await message.AddReactionsAsync(new[] {(IEmote)_approveEmoji, _disproveEmoji});

            var poll = new CachedPromoPoll
            {
                Message = message,
                Campaign = campaign.Campaign,
                Approve = 1,
                Disprove = 0
            };

            _memoryCache.Set(campaign.Campaign.Id, poll);
            _memoryCache.Set(message.Id, poll);
        }

        private async Task DeletePromoPoll(PromotionActionCreationData campaign)
        {
            if (!_memoryCache.TryGetValue(campaign.Campaign.Id, out CachedPromoPoll poll))
                return;
            await poll.Message.DeleteAsync();
        }

        private EmbedBuilder BuildPollEmbed(PromotionCampaignBrief campaign, int Approve, int Disprove)
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

                return new EmbedBuilder()
                    .WithTitle($"#{campaign.Id}")
                    .WithDescription($"**{campaign.Subject.Nickname}** Has been nominated for promotion to {boldRole} ")
                    .WithColor(aviColor.Result)
                    .WithUserAsAuthor(user)
                    .AddField("The current vote total stands at: ", $" {Approve} {_approveEmoji} / {Disprove} {_disproveEmoji}")
                    .WithTimestamp(_utcNow)
                    .WithFooter("Please react with the appropriate reaction to cast your vote!");
        }

        private async Task<ITextChannel> GetPollChannel(IGuild guild)
        {
            var getPollChannel = await _designatedChannelService
                .GetDesignatedChannelsAsync(guild, DesignatedChannelType.PromotionPoll);
            return getPollChannel.First() as ITextChannel;
        }

        private static bool ValidateVoteReaction(IEmote emote)
            => emote.Name == _approveEmoji.Name || emote.Name == _disproveEmoji.Name;
    }
}
