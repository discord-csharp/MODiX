using Discord;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class ChatMessageReceived : INotification
    {
        public ChatMessageReceived(IMessage message)
        {
            Message = message;
        }

        public IMessage Message { get; }
    }
}
