using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Blocklist;
using Modix.Services.Core;
using Serilog;

namespace Modix.Services.Moderation
{
    public class MessageContentCheckBehaviour :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>
    {
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IModerationService _moderationService;
        private readonly IMessageContentPatternService _messageContentPatternService;
        private readonly IDiscordSocketClient _discordSocketClient;

        public MessageContentCheckBehaviour(
            IDesignatedChannelService designatedChannelService,
            IDiscordSocketClient discordSocketClient,
            IAuthorizationService authorizationService,
            IModerationService moderationService, IMessageContentPatternService messageContentPatternService)
        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _authorizationService = authorizationService;
            _moderationService = moderationService;
            _messageContentPatternService = messageContentPatternService;
        }

        public Task HandleNotificationAsync(MessageReceivedNotification notification,
            CancellationToken cancellationToken = default)
            => TryCheckMessageAsync(notification.Message);

        public Task HandleNotificationAsync(MessageUpdatedNotification notification,
            CancellationToken cancellationToken = default)
            => TryCheckMessageAsync(notification.NewMessage);

        private async Task TryCheckMessageAsync(IMessage message)
        {
            if (!(message.Author is IGuildUser author)
                || (author.Guild is null)
                || !(message.Channel is IGuildChannel channel)
                || (channel.Guild is null))
            {
                Log.Debug(
                    "Message {MessageId} was not in an IGuildChannel & IMessageChannel, or Author {Author} was not an IGuildUser",
                    message.Id, message.Author.Id);
                return;
            }

            if (author.Id == _discordSocketClient.CurrentUser.Id)
                return;

            var isContentBlocked = await IsContentBlocked(channel, message);

            if (!isContentBlocked)
            {
                return;
            }

            if (await _designatedChannelService.ChannelHasDesignationAsync(channel.Guild,
                channel, DesignatedChannelType.Unmoderated, default))
            {
                return;
            }

            if (await _authorizationService.HasClaimsAsync(author, AuthorizationClaim.BypassMessageContentPatternCheck))
            {
                Log.Debug("Message {MessageId} was skipped because the author {Author} has the {Claim} claim",
                    message.Id, message.Author.Id, AuthorizationClaim.BypassMessageContentPatternCheck);
                return;
            }

            Log.Debug("Message {MessageId} is going to be deleted", message.Id);

            await _moderationService.DeleteMessageAsync(message, "Unauthorized Message Content Link",
                _discordSocketClient.CurrentUser.Id, default);

            Log.Debug("Message {MessageId} was deleted because it contains blocked content", message.Id);

            await message.Channel.SendMessageAsync($"Sorry {author.Mention} your link has been removed!");
        }

        private async Task<bool> IsContentBlocked(IGuildChannel channel, IMessage message)
        {
            var patterns = await _messageContentPatternService.GetPatterns(channel.GuildId);

            foreach (var patternToCheck in patterns.Where(x => x.Type == MessageContentPatternType.Blocked))
            {
                // If the content is not blocked, we can just continue to check the next
                // blocked pattern
                if (!DoesContentMatchPattern(patternToCheck.Pattern))
                {
                    continue;
                }

                // if we find a block match, we need to check for any exemptions
                foreach (var allowPattern in patterns.Where(x => x.Type == MessageContentPatternType.Allowed))
                {
                    if (DoesContentMatchPattern(allowPattern.Pattern))
                    {
                        return false;
                    }
                }

                return true;

                bool DoesContentMatchPattern(string pattern)
                {
                    var regex = new Regex(
                        pattern: pattern,
                        options: RegexOptions.IgnoreCase,
                        matchTimeout: TimeSpan.FromSeconds(2));

                    var blockMatches = regex.Matches(message.Content);

                    return blockMatches.Any();
                }
            }

            return false;
        }
    }
}
