using System;

using Discord;

using Modix.Common.Messaging;

namespace Modix.Services.AutoRemoveMessage
{
    public class RemovableMessageSentNotification : INotification
    {
        public RemovableMessageSentNotification(IMessage message, IUser user)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public IMessage Message { get; }

        public IUser User { get; }
    }
}
