using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Discord;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Promotions
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to each configured moderation log channel.
    /// </summary>
    public class PromotionLoggingHandler :
        INotificationHandler<PromotionActionCreatedNotification>
    {
        /// <summary>
        /// Constructs a new <see cref="PromotionLoggingHandler"/> object, with injected dependencies.
        /// </summary>
        public PromotionLoggingHandler(
            IAuthorizationService authorizationService,
            IDiscordClient discordClient,
            IDesignatedChannelService designatedChannelService,
            IUserService userService,
            IPromotionsService promotionsService,
            ISelfUser selfUser)
        {
            AuthorizationService = authorizationService;
            DiscordClient = discordClient;
            DesignatedChannelService = designatedChannelService;
            UserService = userService;
            PromotionsService = promotionsService;
            SelfUser = selfUser;
        }
        
        public async Task HandleNotificationAsync(PromotionActionCreatedNotification notification, CancellationToken cancellationToken)
        {
            // TODO: Temporary workaround, remove as part of auth rework.
            if (AuthorizationService.CurrentUserId is null)
                await AuthorizationService.OnAuthenticatedAsync(SelfUser);

            if (await DesignatedChannelService.AnyDesignatedChannelAsync(notification.Data.GuildId, DesignatedChannelType.PromotionLog))
            {
                var message = await FormatPromotionLogEntryAsync(notification.Id);

                if (message == null)
                    return;

                await DesignatedChannelService.SendToDesignatedChannelsAsync(
                    await DiscordClient.GetGuildAsync(notification.Data.GuildId), DesignatedChannelType.PromotionLog, message);
            }

            if (await DesignatedChannelService.AnyDesignatedChannelAsync(notification.Data.GuildId, DesignatedChannelType.PromotionNotifications))
            {
                var embed = await FormatPromotionNotificationAsync(notification.Id, notification.Data);

                if (embed == null)
                    return;

                await DesignatedChannelService.SendToDesignatedChannelsAsync(
                    await DiscordClient.GetGuildAsync(notification.Data.GuildId), DesignatedChannelType.PromotionNotifications, "", embed);
            }
        }

        private async Task<Embed> FormatPromotionNotificationAsync(long promotionActionId, PromotionActionCreationData data)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);
            var targetCampaign = promotionAction.Campaign ?? promotionAction.NewComment.Campaign;

            var embed = new EmbedBuilder();

            if (promotionAction.Type != PromotionActionType.CampaignClosed) { return null; }
            if (targetCampaign.Outcome != PromotionCampaignOutcome.Accepted) { return null; }

            var boldName = $"**{targetCampaign.Subject.GetFullUsername()}**";
            var boldRole = $"**{MentionUtils.MentionRole(targetCampaign.TargetRole.Id)}**";

            var subject = await UserService.GetUserInformationAsync(data.GuildId, targetCampaign.Subject.Id);

            embed = embed
                .WithTitle("The campaign is over!")
                .WithDescription($"Staff accepted the campaign, and {boldName} was promoted to {boldRole}! 🎉")
                .WithAuthor(subject)
                .WithFooter("See more at https://mod.gg/promotions");

            return embed.Build();
        }

        private async Task<string> FormatPromotionLogEntryAsync(long promotionActionId)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);
            var key = (promotionAction.Type, promotionAction.NewComment?.Sentiment, promotionAction.Campaign?.Outcome);

            if (!_logRenderTemplates.TryGetValue(key, out var renderTemplate))
                return null;

            return string.Format(renderTemplate,
                   promotionAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                   promotionAction.Campaign?.Id,
                   promotionAction.Campaign?.Subject.DisplayName,
                   promotionAction.Campaign?.Subject.Id,
                   promotionAction.Campaign?.TargetRole.Name,
                   promotionAction.Campaign?.TargetRole.Id,
                   promotionAction.NewComment?.Campaign.Id,
                   promotionAction.NewComment?.Campaign.Subject.DisplayName,
                   promotionAction.NewComment?.Campaign.Subject.Id,
                   promotionAction.NewComment?.Campaign.TargetRole.Name,
                   promotionAction.NewComment?.Campaign.TargetRole.Id,
                   promotionAction.NewComment?.Content);
        }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> for performing self-authentication.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IDesignatedChannelService"/> for logging moderation actions.
        /// </summary>
        internal protected IDesignatedChannelService DesignatedChannelService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for retrieving user info
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IPromotionsService"/> for performing moderation actions.
        /// </summary>
        internal protected IPromotionsService PromotionsService { get; }

        /// <summary>
        /// The <see cref="ISelfUser"/> representing the bot, within the Discord API.
        /// </summary>
        internal protected ISelfUser SelfUser { get; }

        private static readonly Dictionary<(PromotionActionType, PromotionSentiment?, PromotionCampaignOutcome?), string> _logRenderTemplates
            = new Dictionary<(PromotionActionType, PromotionSentiment?, PromotionCampaignOutcome?), string>()
            {
                { (PromotionActionType.CampaignCreated,  null,                       null),                              "`[{0}]` A campaign (`{1}`) was created to promote **{2}** (`{3}`) to **{4}** (`{5}`)." },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Abstain, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), abstaining from the campaign. ```{11}```" },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Approve, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), approving of the promotion. ```{11}```" },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Oppose,  null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), opposing the promotion. ```{11}```" },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Abstain, null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), abstaining from the campaign. ```{11}```" },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Approve, null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), approving of the promotion. ```{11}```" },
                { (PromotionActionType.CommentModified,  PromotionSentiment.Oppose,  null),                              "`[{0}]` A comment was modified in the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), opposing the promotion. ```{11}```" },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Accepted), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was accepted." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Rejected), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was rejected." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Failed),   "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) failed to process." },
            };
    }
}
