using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Promotions;

namespace Modix.Behaviors
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to each configured moderation log channel.
    /// </summary>
    public class PromotionLoggingBehavior : IPromotionActionEventHandler
    {
        /// <summary>
        /// Constructs a new <see cref="PromotionLoggingBehavior"/> object, with injected dependencies.
        /// </summary>
        public PromotionLoggingBehavior(IServiceProvider serviceProvider, IDiscordClient discordClient, IDesignatedChannelService designatedChannelService)
        {
            DiscordClient = discordClient;
            DesignatedChannelService = designatedChannelService;

            _lazyPromotionsService = new Lazy<IPromotionsService>(() => serviceProvider.GetRequiredService<IPromotionsService>());
        }

        /// <inheritdoc />
        public async Task OnPromotionActionCreatedAsync(long moderationActionId, PromotionActionCreationData data)
        {
            if (!await DesignatedChannelService.AnyDesignatedChannelAsync(data.GuildId, DesignatedChannelType.PromotionLog))
                return;

            try
            {
                var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(moderationActionId);

                if (!_renderTemplates.TryGetValue((promotionAction.Type, promotionAction.Comment?.Sentiment, promotionAction.Campaign?.Outcome), out var renderTemplate))
                    return;

                var message = string.Format(renderTemplate,
                    promotionAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                    promotionAction.Campaign?.Id,
                    promotionAction.Campaign?.Subject.DisplayName,
                    promotionAction.Campaign?.Subject.Id,
                    promotionAction.Campaign?.TargetRole.Name,
                    promotionAction.Campaign?.TargetRole.Id,
                    promotionAction.Comment?.Campaign.Id,
                    promotionAction.Comment?.Campaign.Subject.DisplayName,
                    promotionAction.Comment?.Campaign.Subject.Id,
                    promotionAction.Comment?.Campaign.TargetRole.Name,
                    promotionAction.Comment?.Campaign.TargetRole.Id,
                    promotionAction.Comment?.Content);

                await DesignatedChannelService.SendToDesignatedChannelsAsync(
                    await DiscordClient.GetGuildAsync(data.GuildId), DesignatedChannelType.PromotionLog, message);
            }
            catch (Exception ex)
            {
                var text = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
            }

        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IDesignatedChannelService"/> for logging moderation actions.
        /// </summary>
        internal protected IDesignatedChannelService DesignatedChannelService { get; }

        /// <summary>
        /// An <see cref="IPromotionsService"/> for performing moderation actions.
        /// </summary>
        internal protected IPromotionsService PromotionsService
            => _lazyPromotionsService.Value;
        private readonly Lazy<IPromotionsService> _lazyPromotionsService;

        private static readonly Dictionary<(PromotionActionType, PromotionSentiment?, PromotionCampaignOutcome?), string> _renderTemplates
            = new Dictionary<(PromotionActionType, PromotionSentiment?, PromotionCampaignOutcome?), string>()
            {
                { (PromotionActionType.CampaignCreated,  null,                       null),                              "`[{0}]` A campaign (`{1}`) was created to promote **{2}** (`{3}`) to **{4}** (`{5}`)." },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Abstain, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), abstaining from the campaign. ```{11}```" },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Approve, null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), approving of the promotion. ```{11}```" },
                { (PromotionActionType.CommentCreated,   PromotionSentiment.Oppose,  null),                              "`[{0}]` A comment was added to the campaign (`{6}`) to promote **{7}** (`{8}`) to **{9}** (`{10}`), opposing the promotion. ```{11}```" },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Accepted), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was accepted." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Rejected), "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) was rejected." },
                { (PromotionActionType.CampaignClosed,   null,                       PromotionCampaignOutcome.Failed),   "`[{0}]` The campaign (`{1}`) to promote **{2}** (`{3}`) to **{4}** (`{5}`) failed to process." },
            };
    }
}
