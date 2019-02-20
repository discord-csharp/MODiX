using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Messages;
using Modix.Data.Models.Core;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Data.Utilities;
using Modix.Services.Core;
using Modix.Services.Promotions;

namespace Modix.Behaviors
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to each configured moderation log channel.
    /// </summary>
    public class PromotionLoggingHandler :
        INotificationHandler<PromotionActionCreated>
    {
        /// <summary>
        /// Constructs a new <see cref="PromotionLoggingHandler"/> object, with injected dependencies.
        /// </summary>
        public PromotionLoggingHandler(IServiceProvider serviceProvider, IDiscordClient discordClient, IDesignatedChannelService designatedChannelService)
        {
            DiscordClient = discordClient;
            DesignatedChannelService = designatedChannelService;

            _lazyPromotionsService = new Lazy<IPromotionsService>(() => serviceProvider.GetRequiredService<IPromotionsService>());
        }
        
        public Task Handle(PromotionActionCreated notification, CancellationToken cancellationToken)
            => OnPromotionActionCreatedAsync(notification.PromotionActionId, notification.PromotionActionCreationData);

        public async Task OnPromotionActionCreatedAsync(long promotionActionId, PromotionActionCreationData data)
        {
            if (await DesignatedChannelService.AnyDesignatedChannelAsync(data.GuildId, DesignatedChannelType.PromotionLog))
            {
                var message = await FormatPromotionLogEntry(promotionActionId, data);

                if (message == null)
                    return;

                await DesignatedChannelService.SendToDesignatedChannelsAsync(
                    await DiscordClient.GetGuildAsync(data.GuildId), DesignatedChannelType.PromotionLog, message);
            }

            if (await DesignatedChannelService.AnyDesignatedChannelAsync(data.GuildId, DesignatedChannelType.PromotionNotifications))
            {
                var embed = await FormatPromotionNotification(promotionActionId, data);

                if (embed == null)
                    return;

                await DesignatedChannelService.SendToDesignatedChannelsAsync(
                    await DiscordClient.GetGuildAsync(data.GuildId), DesignatedChannelType.PromotionNotifications, "", embed);
            }
        }

        private async Task<Embed> FormatPromotionNotification(long promotionActionId, PromotionActionCreationData data)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);
            var targetCampaign = promotionAction.Campaign ?? promotionAction.NewComment.Campaign;

            var embed = new EmbedBuilder();

            //Because we have comment creation as a separate operation from starting the campaign,
            //we don't have access to the "initial" comment when a campaign is created. So, we have to
            //note that a campaign was created, and actually send the log message when the first comment
            //is created (containing the comment body)
            switch (promotionAction.Type)
            {
                case PromotionActionType.CampaignCreated:

                    _initialCommentQueue.TryAdd(targetCampaign.Id, embed
                        .WithTitle("A new campaign has been started!")
                        .AddField("If accepted, their new role will be", MentionUtils.MentionRole(targetCampaign.TargetRole.Id)));

                    return null;
                case PromotionActionType.CampaignClosed:

                    var fullCampaign = (await PromotionsService.SearchCampaignsAsync(new PromotionCampaignSearchCriteria
                    {
                        Id = targetCampaign.Id
                    }))
                    .First();

                    embed = embed
                        .WithTitle("The campaign is over!")
                        .AddField("Approval Rate", fullCampaign.GetApprovalPercentage().ToString("p"), true);

                    var boldName = $"**{targetCampaign.Subject.Username}#{targetCampaign.Subject.Discriminator}**";
                    var boldRole = $"**{MentionUtils.MentionRole(targetCampaign.TargetRole.Id)}**";

                    switch (targetCampaign.Outcome)
                    {
                        case PromotionCampaignOutcome.Accepted:
                            embed = embed
                                .WithDescription($"Staff accepted the campaign, and {boldName} was promoted to {boldRole}! 🎉");
                            break;
                        case PromotionCampaignOutcome.Rejected:
                            embed = embed
                                .WithDescription($"Staff rejected the campaign to promote {boldName} to {boldRole}");
                            break;
                        case PromotionCampaignOutcome.Failed:
                        default:
                            embed = embed
                                .WithDescription("There was an issue while accepting or denying the campaign. Ask staff for details.")
                                .AddField("Target Role", MentionUtils.MentionRole(targetCampaign.TargetRole.Id), true);
                            break;
                    }

                    break;
                case PromotionActionType.CommentCreated:

                    if (_initialCommentQueue.TryRemove(targetCampaign.Id, out embed))
                    {
                        embed.Description = $"👍 {promotionAction.NewComment.Content}";
                    }
                    else
                    {
                        return null;
                    }

                    break;
                case PromotionActionType.CommentModified:
                default:
                    return null;
            }

            return embed
                .WithAuthor(await DiscordClient.GetUserAsync(targetCampaign.Subject.Id))
                .WithFooter("See more at https://mod.gg/promotions")
                .Build();
        }

        private async Task<string> FormatPromotionLogEntry(long promotionActionId, PromotionActionCreationData data)
        {
            var promotionAction = await PromotionsService.GetPromotionActionSummaryAsync(promotionActionId);

            if (!_logRenderTemplates.TryGetValue((promotionAction.Type, promotionAction.NewComment?.Sentiment, promotionAction.Campaign?.Outcome), out var renderTemplate))
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

        private static ConcurrentDictionary<long, EmbedBuilder> _initialCommentQueue = new ConcurrentDictionary<long, EmbedBuilder>();

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
