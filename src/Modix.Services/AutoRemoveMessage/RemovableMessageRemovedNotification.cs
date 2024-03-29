using System;

using Discord;

namespace Modix.Services.AutoRemoveMessage
{
    public class RemovableMessageRemovedNotification
    {
        public RemovableMessageRemovedNotification(IMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IMessage Message { get; }
    }
}
