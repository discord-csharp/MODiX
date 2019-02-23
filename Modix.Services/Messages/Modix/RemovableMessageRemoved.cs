using Discord;
using MediatR;

namespace Modix.Services.Messages.Modix
{
    public class RemovableMessageRemoved : INotification
    {
        public IMessage Message { get; set; }
    }
}
