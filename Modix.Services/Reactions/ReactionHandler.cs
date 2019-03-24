using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using MediatR;

using Modix.Data.Models.Reactions;
using Modix.Data.Repositories;
using Modix.Services.Messages.Discord;

namespace Modix.Services.Reactions
{
    /// <summary>
    /// Implements a handler that maintains MODiX's record of reactions.
    /// </summary>
    public sealed class ReactionHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        private readonly IReactionRepository _reactionRepository;

        /// <summary>
        /// Constructs a new <see cref="ReactionHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="discordClient">A client to interact with the Discord API.</param>
        public ReactionHandler(
            IReactionRepository reactionRepository)
        {
            _reactionRepository = reactionRepository;
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
        /// <param name="channel">The channel that the reaction occurred in.</param>
        /// <param name="message">The message that was reacted to.</param>
        /// <param name="reaction">The reaction that was added.</param>
        /// <param name="emote">The emote that was used in the reaction, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task LogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
        {
            using (var transaction = await _reactionRepository.BeginMaintainTransactionAsync())
            {
                await _reactionRepository.CreateAsync(new ReactionCreationData()
                {
                    GuildId = channel.GuildId,
                    ChannelId = channel.Id,
                    MessageId = message.Id,
                    UserId = reaction.UserId,
                    EmojiId = emote?.Id,
                    EmojiName = reaction.Emote.Name,
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
        /// Unlogs a reaction from the database.
        /// </summary>
        /// <param name="channel">The channel that the reaction occurred in.</param>
        /// <param name="message">The message that was reacted to.</param>
        /// <param name="reaction">The reaction that was added.</param>
        /// <param name="emote">The emote that was used in the reaction, if any.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task UnlogReactionAsync(ITextChannel channel, IUserMessage message, SocketReaction reaction, Emote emote)
        {
            using (var transaction = await _reactionRepository.BeginMaintainTransactionAsync())
            {
                await _reactionRepository.DeleteAsync(new ReactionSearchCriteria()
                {
                    GuildId = channel.GuildId,
                    ChannelId = channel.Id,
                    MessageId = message.Id,
                    UserId = reaction.UserId,
                    EmojiId = emote?.Id,
                    EmojiName = emote is null
                        ? reaction.Emote.Name
                        : null,
                });

                transaction.Commit();
            }
        }
    }
}
