using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Behaviors
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to designated moderation log channels.
    /// </summary>
    public class ModerationLoggingBehavior : IModerationActionEventHandler
    {
        private readonly DiscordRelayService _discordRelayService;

        /// <summary>
        /// Constructs a new <see cref="ModerationLoggingBehavior"/> object, with injected dependencies.
        /// </summary>
        public ModerationLoggingBehavior(
            IServiceProvider serviceProvider,
            IDiscordClient discordClient,
            DesignatedChannelService designatedChannelService,
            DiscordRelayService discordRelayService,
            IOptions<ModixConfig> config)
        {
            _discordRelayService = discordRelayService;
            DiscordClient = discordClient;
            DesignatedChannelService = designatedChannelService;
            Config = config.Value;

            _lazyModerationService = new Lazy<ModerationService>(() => serviceProvider.GetRequiredService<ModerationService>());
        }

        /// <inheritdoc />
        public async Task OnModerationActionCreatedAsync(long moderationActionId, ModerationActionCreationData data)
        {
            var designatedChannels = await DesignatedChannelService.GetDesignatedChannelIds(data.GuildId, DesignatedChannelType.ModerationLog);

            if (!designatedChannels.Any())
                return;

            var moderationAction = await ModerationService.GetModerationActionSummaryAsync(moderationActionId);

            if (!_renderTemplates.TryGetValue((moderationAction.Type, moderationAction.Infraction?.Type), out var renderTemplate))
                return;

            // De-linkify links in the message, otherwise Discord will make auto-embeds for them in the log channel
            var content = moderationAction.DeletedMessage?.Content.Replace("http://", "[redacted]").Replace("https://", "[redacted]");

            var message = string.Format(renderTemplate,
                moderationAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                moderationAction.CreatedBy.GetFullUsername(),
                moderationAction.Infraction?.Id,
                moderationAction.Infraction?.Subject.GetFullUsername(),
                moderationAction.Infraction?.Subject.Id,
                moderationAction.Infraction?.Reason,
                moderationAction.DeletedMessage?.Id,
                moderationAction.DeletedMessage?.Author.GetFullUsername(),
                moderationAction.DeletedMessage?.Author.Id,
                moderationAction.DeletedMessage?.Channel.Name ?? moderationAction.DeletedMessages?.First().Channel.Name,
                moderationAction.DeletedMessage?.Channel.Id,
                moderationAction.DeletedMessage?.Reason,
                string.IsNullOrWhiteSpace(content) ? "Empty Message Content" : content,
                moderationAction.DeletedMessages?.Count,
                GetBatchUrl(moderationAction.DeletedMessages?.FirstOrDefault()?.BatchId),
                moderationAction.OriginalInfractionReason,
                string.IsNullOrEmpty(moderationAction.Infraction?.RescindReason) ? "" : $"for reason: ```\n{moderationAction.Infraction?.RescindReason}```");

            foreach (var channel in designatedChannels)
            {
                await _discordRelayService.SendMessageToChannel(channel, message);
            }
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IDesignatedChannelService"/> for logging moderation actions.
        /// </summary>
        internal protected DesignatedChannelService DesignatedChannelService { get; }

        /// <summary>
        /// An <see cref="IModerationService"/> for performing moderation actions.
        /// </summary>
        internal protected ModerationService ModerationService
            => _lazyModerationService.Value;
        private readonly Lazy<ModerationService> _lazyModerationService;

        internal protected static ModixConfig Config { get; private set; }

        // https://modix.gg/logs/deletedMessages?batchId={14}
        private string GetBatchUrl(long? batchId)
            => batchId is null || string.IsNullOrWhiteSpace(Config?.WebsiteBaseUrl)
                ? string.Empty
                : new UriBuilder(Config.WebsiteBaseUrl)
                {
                    Path = "/logs/deletedMessages",
                    Query = $"batchId={batchId}"
                }.RemoveDefaultPort().ToString();

        private static readonly Dictionary<(ModerationActionType, InfractionType?), string> _renderTemplates
            = new Dictionary<(ModerationActionType, InfractionType?), string>()
            {
                { (ModerationActionType.InfractionCreated,   InfractionType.Notice),  "`[{0}]` `[{2}]` **{1}** recorded the following note for **{3}** (`{4}`) ```\n{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Warning), "`[{0}]` `[{2}]` **{1}** recorded the following warning for **{3}** (`{4}`) ```\n{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Mute),    "`[{0}]` `[{2}]` **{1}** muted **{3}** (`{4}`) for reason ```\n{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Ban),     "`[{0}]` `[{2}]` **{1}** banned **{3}** (`{4}`) for reason ```\n{5}```" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Mute),    "`[{0}]` `[{2}]` **{1}** un-muted ** {3}** (`{4}`) {16}" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Ban),     "`[{0}]` `[{2}]` **{1}** un-banned **{3}** (`{4}`) {16}" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Notice),  "`[{0}]` `[{2}]` **{1}** deleted a notice for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Warning), "`[{0}]` `[{2}]` **{1}** deleted a warning for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Mute),    "`[{0}]` `[{2}]` **{1}** deleted a mute for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Ban),     "`[{0}]` `[{2}]` **{1}** deleted a ban for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionUpdated,   InfractionType.Notice),  "`[{0}]` `[{2}]` **{1}** updated a note for **{3}** (`{4}`) from ```\n{15}``` to ```\n{5}```" },
                { (ModerationActionType.InfractionUpdated,   InfractionType.Warning), "`[{0}]` `[{2}]` **{1}** updated a warning  for **{3}** (`{4}`) from ```\n{15}``` to ```\n{5}```" },
                { (ModerationActionType.InfractionUpdated,   InfractionType.Mute),    "`[{0}]` `[{2}]` **{1}** updated a mute for **{3}** (`{4}`) from ```\n{15}``` to ```\n{5}```" },
                { (ModerationActionType.InfractionUpdated,   InfractionType.Ban),     "`[{0}]` `[{2}]` **{1}** updated a ban for **{3}** (`{4}`) from ```\n{15}``` to ```\n{5}```" },
                { (ModerationActionType.MessageDeleted,      null),                   "`[{0}]` **{1}** deleted the following message (`{6}`) from **{7}** (`{8}`) in **#{9}** ```\n{12}``` for reason ```\n{11}```" },
                { (ModerationActionType.MessageBatchDeleted, null),                   "`[{0}]` **{1}** deleted **{13}** messages in **#{9}** (<{14}>)" },
            };
    }
}
