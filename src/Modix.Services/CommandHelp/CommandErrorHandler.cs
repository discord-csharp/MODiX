using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Caching.Memory;

using Modix.Common.Messaging;

namespace Modix.Services.CommandHelp
{
    public class CommandErrorHandler :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<ReactionRemovedNotification>
    {
        private const string AssociatedErrorsKey = nameof(CommandErrorHandler) + ".AssociatedErrors";
        private const string ErrorRepliesKey = nameof(CommandErrorHandler) + ".ErrorReplies";

        //This relates user messages with errors
        private ConcurrentDictionary<ulong, string> AssociatedErrors =>
            _memoryCache.GetOrCreate(AssociatedErrorsKey, _ => new ConcurrentDictionary<ulong, string>());

        //This relates user messages to modix messages containing errors
        private ConcurrentDictionary<ulong, ulong> ErrorReplies =>
            _memoryCache.GetOrCreate(ErrorRepliesKey, _ => new ConcurrentDictionary<ulong, ulong>());

        private const string _emoji = "⚠";
        private readonly IEmote _emote = new Emoji(_emoji);
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IMemoryCache _memoryCache;

        public CommandErrorHandler(DiscordSocketClient discordSocketClient, IMemoryCache memoryCache)
        {
            _discordSocketClient = discordSocketClient;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Associates a user message with an error
        /// </summary>
        /// <param name="message">The message containing an errored command</param>
        /// <param name="error">The error that occurred</param>
        /// <returns></returns>
        public async Task AssociateErrorAsync(IUserMessage message, string error)
        {
            if (AssociatedErrors.TryAdd(message.Id, error))
            {
                await message.AddReactionAsync(new Emoji(_emoji));
            }
        }

        public Task HandleNotificationAsync(ReactionAddedNotification notification, CancellationToken cancellationToken)
            => ReactionAddedAsync(notification.Message, notification.Channel, notification.Reaction);

        public async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            //Don't trigger if the emoji is wrong, if the user is a bot, or if we've
            //made an error message reply already

            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            {
                return;
            }

            if (reaction.Emote.Name != _emoji || ErrorReplies.ContainsKey(cachedMessage.Id))
            {
                return;
            }

            //If the message that was reacted to has an associated error, reply in the same channel
            //with the error message then add that to the replies collection
            if (AssociatedErrors.TryGetValue(cachedMessage.Id, out var value))
            {

                var channel = await cachedChannel.GetOrDownloadAsync();
                var msg = await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = "https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/26a0.png",
                        Name = "That command had an error"
                    },
                    Description = value,
                    Footer = new EmbedFooterBuilder { Text = "Remove your reaction to delete this message" }
                }.Build());

                if (ErrorReplies.TryAdd(cachedMessage.Id, msg.Id) == false)
                {
                    await msg.DeleteAsync();
                }
            }
        }

        public Task HandleNotificationAsync(ReactionRemovedNotification notification, CancellationToken cancellationToken)
            => ReactionRemovedAsync(notification.Message, notification.Channel, notification.Reaction);

        public async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            //Bugfix for NRE?
            if (reaction is null || reaction.User.Value is null)
            {
                return;
            }

            //Don't trigger if the emoji is wrong, or if the user is bot
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            {
                return;
            }

            if (reaction.Emote.Name != _emoji)
            {
                return;
            }

            //If there's an error reply when the reaction is removed, delete that reply,
            //remove the cached error, remove it from the cached replies, and remove
            //the reactions from the original message
            if (ErrorReplies.TryGetValue(cachedMessage.Id, out var botReplyId) == false) { return; }

            var channel = await cachedChannel.GetOrDownloadAsync();
            await channel.DeleteMessageAsync(botReplyId);

            if
            (
                AssociatedErrors.TryRemove(cachedMessage.Id, out _) &&
                ErrorReplies.TryRemove(cachedMessage.Id, out _)
            )
            {
                var originalMessage = await cachedMessage.GetOrDownloadAsync();

                //If we know what user added the reaction, remove their and our reaction
                //Otherwise just remove ours

                if (reaction.User.IsSpecified)
                {
                    await originalMessage.RemoveReactionAsync(_emote, reaction.User.Value);
                }

                await originalMessage.RemoveReactionAsync(_emote, _discordSocketClient.CurrentUser);
            }
        }
    }
}
