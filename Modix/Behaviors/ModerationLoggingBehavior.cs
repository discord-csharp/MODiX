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
    public class ModerationLoggingBehavior : IModerationActionEventHandler
    {
        public ModerationLoggingBehavior(IServiceProvider serviceProvider, IDiscordClient discordClient)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _lazyModerationService = new Lazy<IModerationService>(() => serviceProvider.GetRequiredService<IModerationService>());
        }

        public async Task OnModerationActionCreatedAsync(ulong guildId, long moderationActionId)
        {
            var logChannelIds = await ModerationService.GetLogChannelIdsAsync(guildId);
            if (!logChannelIds.Any())
                return;

            try
            {
                var moderationAction = await ModerationService.GetModerationActionSummaryAsync(moderationActionId);

                if (!_renderTemplates.TryGetValue((moderationAction.Type, moderationAction.Infraction.Type), out var renderTemplate))
                    return;

                var message = string.Format(renderTemplate,
                    moderationAction.Id,
                    moderationAction.Created.UtcDateTime.ToString("HH:mm:ss"),
                    moderationAction.CreatedBy.DisplayName,
                    moderationAction.Infraction.Subject.DisplayName,
                    moderationAction.Infraction.Subject.Id,
                    moderationAction.Infraction.Reason);

                foreach (var logChannelId in logChannelIds)
                    await (await DiscordClient.GetChannelAsync(logChannelId) as IMessageChannel)
                        .SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                var text = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
            }

        }

        internal protected IDiscordClient DiscordClient { get; }

        internal protected IModerationService ModerationService
            => _lazyModerationService.Value;
        private readonly Lazy<IModerationService> _lazyModerationService;

        private static readonly Dictionary<(ModerationActionType, InfractionType), string> _renderTemplates
            = new Dictionary<(ModerationActionType, InfractionType), string>()
            {
                { (ModerationActionType.InfractionCreated,   InfractionType.Notice),  "`[{1}]` **{2}** recorded a note for **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Warning), "`[{1}]` **{2}** recorded a warning for **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Mute),    "`[{1}]` **{2}** muted **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionCreated,   InfractionType.Ban),     "`[{1}]` **{2}** banned **{3}** (`{4}`) ```{5}```" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Mute),    "`[{1}]` **{2}** un-muted ** {3}** (`{4}`)" },
                { (ModerationActionType.InfractionRescinded, InfractionType.Ban),     "`[{1}]` **{2}** un-banned **{3}** (`(4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Notice),  "`[{1}]` **{2}** deleted a notice (`{0}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Warning), "`[{1}]` **{2}** deleted a warning (`{0}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Mute),    "`[{1}]` **{2}** deleted a mute (`{0}`) for **{3}** (`{4}`)" },
                { (ModerationActionType.InfractionDeleted,   InfractionType.Ban),     "`[{1}]` **{2}** deleted a ban (`{0}`) for **{3}** (`{4}`)" },
            };
    }
}
