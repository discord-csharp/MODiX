using System;

using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    public class ModerationActionCreatedEventArgs : EventArgs
    {
        public ModerationActionCreatedEventArgs(ModerationActionEntity action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public ModerationActionEntity Action { get; }
    }
}
