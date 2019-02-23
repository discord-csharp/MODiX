using Discord;
using MediatR;

namespace Modix.Services.Messages.Modix
{
    public class RemovableMessageSent : INotification
    {
        public IMessage Message { get; set; }

        public IUser User { get; set; }
    }
}
