using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Modix.Services.CommandHelp
{
    public class CommandErrorHandler
    {
        private Dictionary<ulong, string> _associatedErrors = new Dictionary<ulong, string>();
        private Dictionary<ulong, ulong> _errorReplies = new Dictionary<ulong, ulong>();

        private const string _emoji = "❓";

        public async Task AssociateError(IUserMessage message, string error)
        {
            await message.AddReactionAsync(new Emoji(_emoji));
            _associatedErrors.Add(message.Id, error);
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || reaction.Emote.Name != _emoji)
            {
                return;
            }

            if (_associatedErrors.TryGetValue(cachedMessage.Id, out string value))
            {
                var msg = await channel.SendMessageAsync($"Error, **{value}**");
                _errorReplies.Add(cachedMessage.Id, msg.Id);
            }
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot || reaction.Emote.Name != _emoji)
            {
                return;
            }

            if (_errorReplies.TryGetValue(cachedMessage.Id, out ulong botReplyId))
            {
                var msg = await channel.GetMessageAsync(botReplyId);
                await msg.DeleteAsync();

                _associatedErrors.Remove(cachedMessage.Id);
                _errorReplies.Remove(cachedMessage.Id);

                var originalMessage = await cachedMessage.GetOrDownloadAsync();
                await originalMessage.RemoveAllReactionsAsync();
            }
        }
    }
}
