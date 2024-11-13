using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Options;

using Modix.Common.Extensions;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Services;
using Modix.Services.Core;
using Modix.Services.Promotions;
using Modix.Services.Utilities;

namespace Modix.Behaviors
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to each configured moderation log channel.
    /// </summary>
    public class PromotionLoggingHandler :
        INotificationHandler<PromotionActionCreatedNotification>
    {
        private readonly DiscordRelayService _discordRelayService;

        /// <summary>
        /// Constructs a new <see cref="PromotionLoggingHandler"/> object, with injected dependencies.
        /// </summary>
        public PromotionLoggingHandler(
            IAuthorizationService authorizationService,
            DiscordSocketClient discordSocketClient,
            DesignatedChannelService designatedChannelService,
            IUserService userService,
            IPromotionsService promotionsService,
            DiscordRelayService discordRelayService,
            IOptions<ModixConfig> modixConfig)
        {
            _discordRelayService = discordRelayService;
            AuthorizationService = authorizationService;
            DiscordSocketClient = discordSocketClient;
            DesignatedChannelService = designatedChannelService;
            UserService = userService;
            PromotionsService = promotionsService;
            ModixConfig = modixConfig.Value;
        }

        public async Task HandleNotificationAsync(PromotionActionCreatedNotification notification, CancellationToken cancellationToken)
        {
            // TODO: Temporary workaround, remove as part of auth rework.
            if (AuthorizationService.CurrentUserId is null)
                await AuthorizationService.OnAuthenticatedAsync(DiscordSocketClient.CurrentUser);

            if (await DesignatedChannelService.HasDesignatedChannelForType(notification.Data.GuildId, DesignatedChannelType.PromotionLog))
            {
                var message = await FormatPromotionLogEntryAsync(notification.Id);

                if (message == null)
                    return;

                var designatedChannels = await DesignatedChannelService.GetDesignatedChannelIds(notification.Data.GuildId,
                    DesignatedChannelType.PromotionLog);

                foreach (var channel in designatedChannels)
                {
                    await _discordRelayService.SendMessageToChannel(channel, message);
                }
            }

            if (await DesignatedChannelService.HasDesignatedChannelForType(notification.Data.GuildId, DesignatedChannelType.PromotionNotifications))
            {
                var embed = await FormatPromotionNotificationAsync(notification.Id, notification.Data);

                if (embed == null)
                    return;

                var designatedChannels = await DesignatedChannelService.GetDesignatedChannelIds(notification.Data.GuildId,
                    DesignatedChannelType.PromotionNotifications);

                foreach (var channel in designatedChannels)
                {
                    await _discordRelayService.SendMessageToChannel(channel, string.Empty, embed);
                }
            }
        }

        private async Task<Embed> FormatPromotionNotificationAsync(long promotionActionId, PromotionActionCreationData data)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);
            var targetCampaign = promotionAction.Campaign ?? promotionAction.NewComment.Campaign;

            var embed = new EmbedBuilder();

            if (promotionAction.Type != PromotionActionType.CampaignClosed)
                return null;
            if (targetCampaign.Outcome != PromotionCampaignOutcome.Accepted)
                return null;

            var boldName = $"**{targetCampaign.Subject.GetFullUsername()}**";
            var boldRole = $"**{MentionUtils.MentionRole(targetCampaign.TargetRole.Id)}**";

            var subject = await UserService.GetUserInformationAsync(data.GuildId, targetCampaign.Subject.Id);

            // https://modix.gg/promotions
            var url = new UriBuilder(ModixConfig.WebsiteBaseUrl)
            {
                Path = "/promotions"
            }.RemoveDefaultPort().ToString();

            embed = embed
                .WithTitle("The campaign is over!")
                .WithDescription($"Staff accepted the campaign, and {boldName} was promoted to {boldRole}! 🎉")
                .WithUserAsAuthor(subject)
                .WithFooter($"See more at {url}");

            return embed.Build();
        }

        private async Task<string> FormatPromotionLogEntryAsync(long promotionActionId)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);
            var key = (promotionAction.Type, promotionAction.NewComment?.Sentiment, promotionAction.Campaign?.Outcome);

            if (!_logRenderTemplates.TryGetValue(key, out var renderTemplate))
                return null;

            var logMessage = string.Format(renderTemplate,
                   promotionAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                   promotionAction.Campaign?.Id,
                   promotionAction.Campaign?.Subject.GetFullUsername(),
                   promotionAction.Campaign?.Subject.Id,
                   promotionAction.Campaign?.TargetRole.Name,
                   promotionAction.Campaign?.TargetRole.Id,
                   promotionAction.NewComment?.Campaign.Id,
                   promotionAction.NewComment?.Campaign.Subject.GetFullUsername(),
                   promotionAction.NewComment?.Campaign.Subject.Id,
                   promotionAction.NewComment?.Campaign.TargetRole.Name,
                   promotionAction.NewComment?.Campaign.TargetRole.Id);

            if (!string.IsNullOrWhiteSpace(promotionAction.NewComment?.Content))
                logMessage += $" ```{promotionAction.NewComment.Content}```";

            return logMessage;
        }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> for performing self-authentication.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordSocketClient { get; }

        /// <summary>
        /// An <see cref="IDesignatedChannelService"/> for logging moderation actions.
        /// </summary>
        internal protected DesignatedChannelService DesignatedChannelService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for retrieving user info
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IPromotionsService"/> for performing moderation actions.
        /// </summary>
        internal protected IPromotionsService PromotionsService { get; }

        /// <summary>
        /// A <see cref="ModixConfig"/> for interacting with the bot configuration.
        /// </summary>
        internal protected ModixConfig ModixConfig  { get; }

        private static readonly Dictionary<(PromotionActionType, PromotionSentiment?, PromotionCampaignOutcome?), string> _logRenderTemplates
            = new()
            {
                { (PromotionActionType.CampaignCreated,  null,                       null),                              "`[{0}]` A campaign (`{1}`) was created to promote **{2}** (`{3}`) to **{4}** (`{5}`)." },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Abstain, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), abstaining from the campaign." },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Approve, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), approving of the promotion." },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Oppose,  null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), opposing the promotion." },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Abstain, null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), abstaining from the campaign." },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Approve, null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), approving of the promotion." },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Oppose,  null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), opposing the promotion." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Accepted), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was accepted." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Rejected), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was rejected." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Failed),   "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) failed to process." },
            };
    }
}
