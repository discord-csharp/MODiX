using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Services.Utilities;

namespace Modix.Bot.Responders
{
    public sealed class EmojiUseResponder(IEmojiRepository emojiRepository) :
        INotificationHandler<ReactionAddedNotificationV3>,
        INotificationHandler<ReactionRemovedNotificationV3>,
        INotificationHandler<MessageReceivedNotificationV3>,
        INotificationHandler<MessageUpdatedNotificationV3>,
        INotificationHandler<MessageDeletedNotificationV3>
    {
        private static readonly Regex _emojiRegex = new($@"(<a?:\w+:[0-9]+>|{EmojiUtilities.EmojiPattern})", RegexOptions.Compiled);

        public async Task Handle(ReactionAddedNotificationV3 notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = (ITextChannel)await notification.Channel.GetOrDownloadAsync();
            if (channel is null)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is not { Author: { IsBot: false } })
                return;

            var reaction = notification.Reaction;
            if (reaction is null)
                return;

            var emote = reaction.Emote as Emote;

            await LogReactionAsync(channel, message, reaction, emote);
        }

        private async Task LogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
        {
            using var transaction = await emojiRepository.BeginMaintainTransactionAsync();

            await emojiRepository.CreateAsync(new EmojiCreationData()
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                MessageId = message.Id,
                UserId = reaction.UserId,
                EmojiId = emote?.Id,
                EmojiName = reaction.Emote.Name,
                IsAnimated = emote?.Animated ?? false,
                UsageType = EmojiUsageType.Reaction,
            });

            transaction.Commit();
        }

        public async Task Handle(ReactionRemovedNotificationV3 notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = (ITextChannel)await notification.Channel.GetOrDownloadAsync();
            if (channel is null)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is not { Author: { IsBot: false } })
                return;

            var reaction = notification.Reaction;
            if (reaction is null)
                return;

            var emote = reaction.Emote as Emote;

            await UnlogReactionAsync(channel, message, reaction, emote);
        }

        private async Task UnlogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
        {
            using var transaction = await emojiRepository.BeginMaintainTransactionAsync();

            await emojiRepository.DeleteAsync(new EmojiSearchCriteria()
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                MessageId = message.Id,
                UserId = reaction.UserId,
                EmojiId = emote?.Id,
                EmojiName = emote is null
                    ? reaction.Emote.Name
                    : null,
                UsageType = EmojiUsageType.Reaction,
            });

            transaction.Commit();
        }

        public async Task Handle(MessageReceivedNotificationV3 notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (notification.Message.Channel is not ITextChannel channel)
                return;

            if (notification.Message is not { Author: { IsBot: false }, Content: not null } message)
                return;

            var newEmoji = _emojiRegex.Matches(message.Content);

            if (newEmoji.Count == 0)
                return;

            foreach (var (emoji, count) in newEmoji.GroupBy(x => x.Value).Select(x => (x.Key, x.Count())))
            {
                var isEmote = Emote.TryParse(emoji, out var emote);

                await LogMultipleMessageEmojiAsync(channel, message, isEmote ? emote.Name : emoji, isEmote ? emote : null, count);
            }
        }

        public async Task Handle(MessageUpdatedNotificationV3 notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrWhiteSpace(notification.NewMessage.Content))
                return;

            if (notification.Channel is not ITextChannel channel)
                return;

            await UnlogMessageContentEmojiAsync(channel, notification.OldMessage.Id);

            if (notification.NewMessage is not { Author: { IsBot: false }, Content: not null } newMessage)
                return;

            var newEmoji = _emojiRegex.Matches(newMessage.Content);

            if (newEmoji.Count == 0)
                return;

            foreach (var (emoji, count) in newEmoji.Cast<Match>().GroupBy(x => x.Value).Select(x => (x.Key, x.Count())))
            {
                var isEmote = Emote.TryParse(emoji, out var emote);

                await LogMultipleMessageEmojiAsync(channel, newMessage, isEmote ? emote.Name : emoji, isEmote ? emote : null, count);
            }
        }

        public async Task Handle(MessageDeletedNotificationV3 notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is not { Author: { IsBot: false } })
                return;

            var channel = (ITextChannel)await notification.Channel.GetOrDownloadAsync();

            await UnlogAllEmojiAsync(channel, notification.Message.Id);
        }

        private async Task LogMultipleMessageEmojiAsync(ITextChannel channel, IMessage message, string emoji, Emote emote, int count)
        {
            using var transaction = await emojiRepository.BeginMaintainTransactionAsync();

            await emojiRepository.CreateMultipleAsync(new EmojiCreationData()
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                MessageId = message.Id,
                UserId = message.Author.Id,
                EmojiId = emote?.Id,
                EmojiName = emoji,
                IsAnimated = emote?.Animated ?? false,
                UsageType = EmojiUsageType.MessageContent,
            },
            count);

            transaction.Commit();
        }

        private async Task UnlogMessageContentEmojiAsync(ITextChannel channel, ulong messageId)
        {
            using var transaction = await emojiRepository.BeginMaintainTransactionAsync();

            await emojiRepository.DeleteAsync(new EmojiSearchCriteria()
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                MessageId = messageId,
                UsageType = EmojiUsageType.MessageContent,
            });

            transaction.Commit();
        }

        private async Task UnlogAllEmojiAsync(ITextChannel channel, ulong messageId)
        {
            using var transaction = await emojiRepository.BeginMaintainTransactionAsync();

            await emojiRepository.DeleteAsync(new EmojiSearchCriteria()
            {
                GuildId = channel.GuildId,
                ChannelId = channel.Id,
                MessageId = messageId,
            });

            transaction.Commit();
        }
    }
}
