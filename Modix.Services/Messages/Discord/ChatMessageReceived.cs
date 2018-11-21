using Discord;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ChatMessageReceived : INotification
    {
        public IMessage Message { get; set; }
    }
}
