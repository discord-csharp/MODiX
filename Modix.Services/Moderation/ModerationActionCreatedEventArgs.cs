using System;

using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    public class ModerationActionCreatedEventArgs : EventArgs
    {
        public ModerationActionCreatedEventArgs(ModerationAction action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public ModerationAction Action { get; }
    }
}
