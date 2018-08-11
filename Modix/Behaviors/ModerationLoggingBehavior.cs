using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.Services.Moderation;

namespace Modix.Behaviors
{
    /// <summary>
    /// Renders moderation actions, as they are created, as messages to each configured moderation log channel.
    /// </summary>
    public class ModerationLoggingBehavior : IModerationActionEventHandler
    {
        /// <summary>
        /// Constructs a new <see cref="ModerationLoggingBehavior"/> object, with injected dependencies.
        /// </summary>
        public ModerationLoggingBehavior(IServiceProvider serviceProvider, IDiscordClient discordClient)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _lazyModerationService = new Lazy<IModerationService>(() => serviceProvider.GetRequiredService<IModerationService>());
        }

        /// <inheritdoc />
        public async Task OnModerationActionCreatedAsync(long moderationActionId, ModerationActionCreationData data)
        {
            var logChannelIds = await ModerationService.GetLogChannelIdsAsync(data.GuildId);
            if (!logChannelIds.Any())
                return;

            try
            {
                var moderationAction = await ModerationService.GetModerationActionSummaryAsync(moderationActionId);

                if (!_renderTemplates.TryGetValue((moderationAction.Type, moderationAction.Infraction?.Type), out var renderTemplate))
                    return;

                var message = string.Format(renderTemplate,
                    moderationAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                    moderationAction.CreatedBy.DisplayName,
                    moderationAction.Infraction?.Id,
                    moderationAction.Infraction?.Subject.DisplayName,
                    moderationAction.Infraction?.Subject.Id,
                    moderationAction.Infraction?.Reason,
                    moderationAction.DeletedMessage?.Id,
                    moderationAction.DeletedMessage?.Author.DisplayName,
                    moderationAction.DeletedMessage?.Author.Id,
                    moderationAction.DeletedMessage?.Channel.Name,
                    moderationAction.DeletedMessage?.Channel.Id,
                    moderationAction.DeletedMessage?.Reason,
                    // De-linkify links in the message, otherwise Discord will make auto-embeds for them in the log channel
                    moderationAction.DeletedMessage?.Content.Replace("http://", "[redacted]").Replace("https://", "[redacted]"));

                foreach (var logChannelId in logChannelIds)
                    await (await DiscordClient.GetChannelAsync(logChannelId) as IMessageChannel)
                        .SendMessageAsync(message);
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
        /// An <see cref="IModerationService"/> for performing moderation actions.
        /// </summary>
        internal protected IModerationService ModerationService
            => _lazyModerationService.Value;
        private readonly Lazy<IModerationService> _lazyModerationService;

        private static readonly Dictionary<(ModerationActionType, InfractionType?), string> _renderTemplates
            = new Dictionary<(ModerationActionType, InfractionType?), string>()
            {
                { (ModerationActionType.InfractionCreated,   InfractionType.Notice),  "`[{0}]` **{1}** recorded the following note for **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Warning), "`[{0}]` **{1}** recorded the following warning for **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Mute),    "`[{0}]` **{1}** muted **{3}** (`{4}`) for reason ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Ban),     "`[{0}]` **{1}** banned **{3}** (`{4}`) for reason ```{5}```" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Mute),    "`[{0}]` **{1}** un-muted ** {3}** (`{4}`)" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Ban),     "`[{0}]` **{1}** un-banned **{3}** (`(4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Notice),  "`[{0}]` **{1}** deleted a notice (`{2}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Warning), "`[{0}]` **{1}** deleted a warning (`{2}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Mute),    "`[{0}]` **{1}** deleted a mute (`{2}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Ban),     "`[{0}]` **{1}** deleted a ban (`{2}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.MessageDeleted,      null),                   "`[{0}]` **{1}** deleted the following message (`{6}`) from **{7}** (`{8}`) in **#{9}** ```{12}``` for reason ```{11}```" },
            };
    }
}
