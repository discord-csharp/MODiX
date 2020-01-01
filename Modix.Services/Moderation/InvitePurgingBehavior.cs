using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Discord;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;

using Serilog;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Listens for Discord Invite links posted into moderated channels,
    /// and removes them if the users posting them are not allowed to do so.
    /// </summary>
    public class InvitePurgingBehavior :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>
    {
        /// <summary>
        /// Constructs a new <see cref="InvitePurgingBehavior"/> object, with the given injected dependencies.
        /// </summary>
        public InvitePurgingBehavior(
            IDesignatedChannelService designatedChannelService,
            IAuthorizationService authorizationService,
            IModerationService moderationService,
            ISelfUserProvider selfUserProvider,
            IDiscordClient discordClient)
        {
            DesignatedChannelService = designatedChannelService;
            AuthorizationService = authorizationService;
            ModerationService = moderationService;
            SelfUserProvider = selfUserProvider;
            DiscordClient = discordClient;
        }

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken = default)
            => TryPurgeInviteLinkAsync(notification.Message);

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageUpdatedNotification notification, CancellationToken cancellationToken = default)
            => TryPurgeInviteLinkAsync(notification.NewMessage);

        /// <summary>
        /// An <see cref="IDesignatedChannelService"/> used to determine if a channel should be monitored for invite links.
        /// </summary>
        internal protected IDesignatedChannelService DesignatedChannelService { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> used to determine if a user is authorized to post invite links
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IModerationService"/> used to delete messages, with associated moderation logging.
        /// </summary>
        internal protected IModerationService ModerationService { get; }

        /// <summary>
        /// An <see cref="ISelfUserProvider"/> used to interact with the current bot user.
        /// </summary>
        internal protected ISelfUserProvider SelfUserProvider { get; }

        internal protected IDiscordClient DiscordClient { get; }

        private async Task TryPurgeInviteLinkAsync(IMessage message)
        {
            if
            (
                !(message.Author is IGuildUser author) || (author.Guild is null) ||
                !(message.Channel is IGuildChannel channel) || (channel.Guild is null)
            )
            {
                Log.Debug("Message {MessageId} was not in an IGuildChannel & IMessageChannel, or Author {Author} was not an IGuildUser",
                    message.Id, message.Author.Id);
                return;
            }

            var selfUser = await SelfUserProvider.GetSelfUserAsync();
            if (author.Id == selfUser.Id)
                return;

            var matches = _inviteLinkMatcher.Matches(message.Content);
            if (!matches.Any())
                return;

            if (await DesignatedChannelService.ChannelHasDesignationAsync(channel.Guild, channel, DesignatedChannelType.Unmoderated))
                return;

            if (await AuthorizationService.HasClaimsAsync(author, AuthorizationClaim.PostInviteLink))
            {
                Log.Debug("Message {MessageId} was skipped because the author {Author} has the PostInviteLink claim",
                    message.Id, message.Author.Id);
                return;
            }

            var invites = new List<IInvite>(matches.Count);

            foreach (var code in matches.Select(x => x.Groups["Code"].Value))
            {
                var invite = await DiscordClient.GetInviteAsync(code);
                invites.Add(invite);
            }

            // Allow invites to the guild in which the message was posted
            if (invites.All(x => x?.GuildId == author.GuildId))
            {
                Log.Debug("Message {MessageId} was skipped because the invite was to this server", message.Id);
                return;
            }

            Log.Debug("Message {MessageId} is going to be deleted", message.Id);

            await ModerationService.DeleteMessageAsync(message, "Unauthorized Invite Link", selfUser.Id);

            Log.Debug("Message {MessageId} was deleted because it contains an invite link", message.Id);

            await message.Channel.SendMessageAsync($"Sorry {author.Mention} your invite link has been removed - please don't post links to other guilds");
        }

        private static readonly Regex _inviteLinkMatcher
            = new Regex(
                pattern: @"(https?://)?(www\.)?(discord\.(gg|io|me|li)|discordapp\.com/invite)/(?<Code>\w+)",
                options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                matchTimeout: TimeSpan.FromSeconds(2));
    }
}
