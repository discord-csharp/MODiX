using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ChatMessageUpdated : INotification
    {
        public Cacheable<IMessage, ulong> OldMessage { get; set; }
        public IMessage NewMessage { get; set; }
        public ISocketMessageChannel Channel { get; set; }
    }
}
