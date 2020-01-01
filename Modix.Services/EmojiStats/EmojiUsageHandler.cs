﻿using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Services.Utilities;

namespace Modix.Services.EmojiStats
{
    /// <summary>
    /// Implements a handler that maintains MODiX's record of emoji.
    /// </summary>
    public sealed class EmojiUsageHandler :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<ReactionRemovedNotification>,
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageUpdatedNotification>,
        INotificationHandler<MessageDeletedNotification>
    {
        private readonly IEmojiRepository _emojiRepository;

        public static readonly Regex EmojiRegex = new Regex($@"(<a?:\w+:[0-9]+>|{EmojiUtilities.EmojiPattern})", RegexOptions.Compiled);

        /// <summary>
        /// Constructs a new <see cref="EmojiUsageHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="emojiRepository">Repository for managing emoji entities within an underlying data storage provider.</param>
        public EmojiUsageHandler(
            IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }

        public async Task HandleNotificationAsync(ReactionAddedNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = notification.Channel as ITextChannel;
            if (channel is null)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is null)
                return;

            if (message.Author.IsBot)
                return;

            var reaction = notification.Reaction;
            if (reaction is null)
                return;

            var emote = reaction.Emote as IEmoteEntity;

            await LogReactionAsync(channel, message, reaction, emote);
        }

        /// <summary>
        /// Logs a reaction in the database.
        /// </summary>
        /// <param name="channel">The channel that the emoji was used in.</param>
        /// <param name="message">The message associated with the emoji.</param>
        /// <param name="reaction">The emoji that was used.</param>
        /// <param name="emote">The emote that was used, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task LogReactionAsync(ITextChannel channel, IUserMessage message, ISocketReaction reaction, IEmoteEntity emote)
        {
            using (var transaction = await _emojiRepository.BeginMaintainTransactionAsync())
            {
                await _emojiRepository.CreateAsync(new EmojiCreationData()
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
        }

        public async Task HandleNotificationAsync(ReactionRemovedNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = notification.Channel as ITextChannel;
            if (channel is null)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is null)
                return;

            if (message.Author.IsBot)
                return;

            var reaction = notification.Reaction;
            if (reaction is null)
                return;

            var emote = reaction.Emote as IEmoteEntity;

            await UnlogReactionAsync(channel, message, reaction, emote);
        }

        /// <summary>
        /// Unlogs an emoji from the database.
        /// </summary>
        /// <param name="channel">The channel that the emoji was used in.</param>
        /// <param name="message">The message associated with the emoji.</param>
        /// <param name="reaction">The emoji that was used.</param>
        /// <param name="emote">The emote that was used, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task UnlogReactionAsync(ITextChannel channel, IUserMessage message, ISocketReaction reaction, IEmoteEntity emote)
        {
            using (var transaction = await _emojiRepository.BeginMaintainTransactionAsync())
            {
                await _emojiRepository.DeleteAsync(new EmojiSearchCriteria()
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
        }

        public async Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var newEmoji = EmojiRegex.Matches(notification.Message.Content);

            if (newEmoji.Count == 0)
                return;

            var channel = notification.Message.Channel as ITextChannel;
            var message = notification.Message as IUserMessage;

            if (message.Author.IsBot)
                return;

            foreach (var (emoji, count) in newEmoji.Cast<Match>().GroupBy(x => x.Value).Select(x => (x.Key, x.Count())))
            {
                var isEmote = Emote.TryParse(emoji, out var emote);

                await LogMultipleMessageEmojiAsync(channel, message, isEmote ? emote.Name : emoji, isEmote ? emote : null, count);
            }
        }

        public async Task HandleNotificationAsync(MessageUpdatedNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (string.IsNullOrWhiteSpace(notification.NewMessage.Content))
                return;

            var channel = notification.Channel as ITextChannel;

            await UnlogMessageContentEmojiAsync(channel, notification.OldMessage.Id);

            var newMessage = notification.NewMessage as IUserMessage;
            var newEmoji = EmojiRegex.Matches(newMessage.Content);

            if (newEmoji.Count == 0)
                return;

            if (newMessage.Author.IsBot)
                return;

            foreach (var (emoji, count) in newEmoji.Cast<Match>().GroupBy(x => x.Value).Select(x => (x.Key, x.Count())))
            {
                var isEmote = Emote.TryParse(emoji, out var emote);

                await LogMultipleMessageEmojiAsync(channel, newMessage, isEmote ? emote.Name : emoji, isEmote ? emote : null, count);
            }
        }

        public async Task HandleNotificationAsync(MessageDeletedNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var message = await notification.Message.GetOrDownloadAsync();
            if (message is { Author: { IsBot: true } })
                return;

            var channel = notification.Channel as ITextChannel;

            await UnlogAllEmojiAsync(channel, notification.Message.Id);
        }

        private async Task LogMultipleMessageEmojiAsync(ITextChannel channel, IUserMessage message, string emoji, Emote emote, int count)
        {
            using (var transaction = await _emojiRepository.BeginMaintainTransactionAsync())
            {
                await _emojiRepository.CreateMultipleAsync(new EmojiCreationData()
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
        }

        private async Task UnlogMessageContentEmojiAsync(ITextChannel channel, ulong messageId)
        {
            using (var transaction = await _emojiRepository.BeginMaintainTransactionAsync())
            {
                await _emojiRepository.DeleteAsync(new EmojiSearchCriteria()
                {
                    GuildId = channel.GuildId,
                    ChannelId = channel.Id,
                    MessageId = messageId,
                    UsageType = EmojiUsageType.MessageContent,
                });

                transaction.Commit();
            }
        }

        private async Task UnlogAllEmojiAsync(ITextChannel channel, ulong messageId)
        {
            using (var transaction = await _emojiRepository.BeginMaintainTransactionAsync())
            {
                await _emojiRepository.DeleteAsync(new EmojiSearchCriteria()
                {
                    GuildId = channel.GuildId,
                    ChannelId = channel.Id,
                    MessageId = messageId,
                });

                transaction.Commit();
            }
        }
    }
}
