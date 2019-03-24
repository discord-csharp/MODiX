using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using MediatR;

using Modix.Data.Models.Emoji;
using Modix.Data.Repositories;
using Modix.Services.Messages.Discord;

namespace Modix.Services.EmojiStats
{
    /// <summary>
    /// Implements a handler that maintains MODiX's record of emoji.
    /// </summary>
    public sealed class EmojiUsageHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        private readonly IEmojiRepository _emojiRepository;

        /// <summary>
        /// Constructs a new <see cref="EmojiUsageHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">A client to interact with the Discord API.</param>
        public EmojiUsageHandler(
            IEmojiRepository emojiRepository)
        {
            _emojiRepository = emojiRepository;
        }

        public async Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = notification.Channel as ITextChannel;
            var message = await notification.Message.GetOrDownloadAsync();
            var reaction = notification.Reaction;
            var emote = reaction.Emote as Emote;

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
        private async Task LogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
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
                    UsageType = EmojiUsageType.Reaction,
                });

                transaction.Commit();
            }
        }

        public async Task Handle(ReactionRemoved notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var channel = notification.Channel as ITextChannel;
            var message = await notification.Message.GetOrDownloadAsync();
            var reaction = notification.Reaction;
            var emote = reaction.Emote as Emote;

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
        private async Task UnlogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
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
    }
}
