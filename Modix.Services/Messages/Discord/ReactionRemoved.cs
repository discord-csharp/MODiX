using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ReactionRemoved : INotification
    {
        public Cacheable<IUserMessage, ulong> Message { get; set; }
        public ISocketMessageChannel Channel { get; set; }
        public SocketReaction Reaction { get; set; }
    }
}
