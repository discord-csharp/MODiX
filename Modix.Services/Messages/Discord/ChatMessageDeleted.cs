using Discord;
using Discord.WebSocket;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ChatMessageDeleted : INotification
    {
        public ISocketMessageChannel Channel { get; set; }
    
        public Cacheable<IMessage, ulong> Message { get; set; }
}
}
