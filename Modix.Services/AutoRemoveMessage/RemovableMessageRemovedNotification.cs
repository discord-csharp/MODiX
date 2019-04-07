using System;

using Discord;

using Modix.Common.Messaging;

namespace Modix.Services.AutoRemoveMessage
{
    public class RemovableMessageRemovedNotification : INotification
    {
        public RemovableMessageRemovedNotification(IMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public IMessage Message { get; }
    }
}
