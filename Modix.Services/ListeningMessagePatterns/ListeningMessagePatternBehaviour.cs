using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Common.Messaging;
using Serilog;

namespace Modix.Services.ListeningMessagePatterns
{
    public class ListeningMessagePatternBehaviour :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>
    {
        private readonly IListeningMessagePatternService _messageContentPatternService;
        private readonly DiscordSocketClient _discordSocketClient;

        public ListeningMessagePatternBehaviour(IListeningMessagePatternService messageContentPatternService, DiscordSocketClient discordSocketClient)
        {
            _messageContentPatternService = messageContentPatternService;
            _discordSocketClient = discordSocketClient;
        }

        public Task HandleNotificationAsync(MessageReceivedNotification notification,
            CancellationToken cancellationToken = default)
            => HandleMessage(notification.Message);

        public Task HandleNotificationAsync(MessageUpdatedNotification notification,
            CancellationToken cancellationToken = default)
            => HandleMessage(notification.NewMessage);

        private async Task HandleMessage(IMessage message)
        {
            if (message.Author is not IGuildUser author
                || (author.Guild is null)
                || message.Channel is not IGuildChannel channel
                || (channel.Guild is null))
            {
                Log.Debug(
                    "Message {MessageId} was not in an IGuildChannel & IMessageChannel, or Author {Author} was not an IGuildUser",
                    message.Id, message.Author.Id);
                return;
            }

            if (author.Id == _discordSocketClient.CurrentUser.Id)
                return;

            var catchAll = await _messageContentPatternService.GetGuildCatchAllPatternAsync(channel.GuildId);
            bool lookingForTimeoutCause = false;
            try
            {
                var catchAllMatch = catchAll.Match(message.CleanContent);
                if (!catchAllMatch.Success)
                    return;
            }
            catch (RegexMatchTimeoutException)
            {
                lookingForTimeoutCause = true;
            }

            // matched, or timeout.

            var patterns = await _messageContentPatternService.GetGuildPatternsAsync(channel.GuildId);

            foreach (var pattern in patterns)
            {
                try
                {
                    var match = pattern.Regex.Match(message.CleanContent);
                    if (match.Success)
                    {
                        await HandleMessageMatchAsync(message, match);
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    lookingForTimeoutCause = false;
                    await _messageContentPatternService.DisablePattern(channel.GuildId, pattern.Pattern);
                    TODO // Log this in dedicated channel.
                }
            }

            if(lookingForTimeoutCause)
            {
                TODO // Did not found cause to the global regex timeout.
            }
        }

        private Task HandleMessageMatchAsync(IMessage message, Match match)
            => throw new NotImplementedException();
    }
}
