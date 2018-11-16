using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ChatMessageUpdated : INotification
    {
        public Cacheable<IMessage, ulong> OldMessage { get; }
        public IMessage NewMessage { get; }
        public ISocketMessageChannel Channel { get; }

        public ChatMessageUpdated(Cacheable<IMessage, ulong> oldMessage, IMessage newMessage, ISocketMessageChannel channel)
        {
            OldMessage = oldMessage;
            NewMessage = newMessage;
            Channel = channel;
        }
    }
}
