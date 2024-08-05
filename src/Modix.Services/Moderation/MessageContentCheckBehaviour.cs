using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.MessageContentPatterns;
using Serilog;

namespace Modix.Services.Moderation
{
    public class MessageContentCheckBehaviour :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>,
        INotificationHandler<VoiceChannelStatusUpdatedNotification>
    {
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IModerationService _moderationService;
        private readonly IMessageContentPatternService _messageContentPatternService;
        private readonly DiscordSocketClient _discordSocketClient;

        public MessageContentCheckBehaviour(
            IDesignatedChannelService designatedChannelService,
            DiscordSocketClient discordSocketClient,
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
        public async Task HandleNotificationAsync(VoiceChannelStatusUpdatedNotification notification,
            CancellationToken cancellationToken = default)
        {
            var channel = await notification.Channel.GetOrDownloadAsync();
            var newStatus = notification.NewStatus;
            var isBlocked = await IsContentBlocked(channel, newStatus);

            if (!isBlocked)
            {
                return;
            }

            Log.Debug("Status {newStatus} from voice channel {channelId} is going to be deleted", newStatus, channel.Id);

            //Setting to old status seems risky as there is a race condition
            //between two consecutive edits being moderated in parallel.
            //May client events being fired in parallel?
            await channel.SetStatusAsync(string.Empty);

            Log.Debug("Status {newStatus} from voice channel {channelId} was deleted because it contains blocked content", newStatus, channel.Id);

            await channel.SendMessageAsync("Sorry, the new status contained blocked content and has been removed!");
        }

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

            if (await _designatedChannelService.ChannelHasDesignationAsync(channel.Guild.Id,
                channel.Id, DesignatedChannelType.Unmoderated, default))
            {
                return;
            }

            if (await _authorizationService.HasClaimsAsync(author.Id, author.Guild.Id, author.RoleIds?.ToList(), AuthorizationClaim.BypassMessageContentPatternCheck))
            {
                Log.Debug("Message {MessageId} was skipped because the author {Author} has the {Claim} claim",
                    message.Id, message.Author.Id, AuthorizationClaim.BypassMessageContentPatternCheck);
                return;
            }

            Log.Debug("Message {MessageId} is going to be deleted", message.Id);

            await _moderationService.DeleteMessageAsync(message, "Unauthorized Message Content",
                _discordSocketClient.CurrentUser.Id, default);

            Log.Debug("Message {MessageId} was deleted because it contains blocked content", message.Id);

            await message.Channel.SendMessageAsync(
                $"Sorry {author.Mention} your message contained blocked content and has been removed!");
        }

        private Task<bool> IsContentBlocked(SocketVoiceChannel channel, string status) =>
            IsContentBlocked(channel.Guild.Id, status);
        private Task<bool> IsContentBlocked(IGuildChannel channel, IMessage message) =>
            IsContentBlocked(channel.GuildId, message.Content);
        private async Task<bool> IsContentBlocked(ulong guildId, string messageContent)
        {
            var patterns = await _messageContentPatternService.GetPatterns(guildId);

            foreach (var patternToCheck in patterns.Where(x => x.Type == MessageContentPatternType.Blocked))
            {
                // If the content is not blocked, we can just continue to check the next
                // blocked pattern

                var (containsBlockedPattern, blockedMatches) = GetContentMatches(messageContent, patternToCheck);

                if (!containsBlockedPattern)
                {
                    continue;
                }

                var allowedPatterns = patterns.Where(x => x.Type == MessageContentPatternType.Allowed).ToList();

                if (!allowedPatterns.Any())
                {
                    return true;
                }

                foreach (Match blockedMatch in blockedMatches)
                {
                    var didFindAllowedPattern = false;

                    foreach (var allowPattern in allowedPatterns)
                    {
                        var (hasAllowedMatch, _) = GetContentMatches(blockedMatch.Value, allowPattern);
                        didFindAllowedPattern = hasAllowedMatch;
                    }

                    if (!didFindAllowedPattern)
                        return true;
                }

                return false;

                static (bool, MatchCollection) GetContentMatches(string content, MessageContentPatternDto pattern)
                {
                    var matches = pattern.Regex.Matches(content);
                    return (matches.Any(), matches);
                }
            }

            return false;
        }
    }
}
