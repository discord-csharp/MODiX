using System;

using Discord;

using Modix.Common.Messaging;

namespace Modix.Services.AutoRemoveMessage
{
    public class RemovableMessageSentNotification : INotification
    {
        public RemovableMessageSentNotification(IMessage message, IUser[] users)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public IMessage Message { get; }

        public IUser[] Users { get; }
    }
}
