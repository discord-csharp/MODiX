using System;

using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Describes data about the <see cref="IModerationService.ModerationActionCreated"/> event.
    /// </summary>
    public class ModerationActionCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionCreatedEventArgs"/> object.
        /// </summary>
        /// <param name="action">The value to use for <see cref="Action"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="action"/>.</exception>
        public ModerationActionCreatedEventArgs(ModerationActionSummary action)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> that was created.
        /// </summary>
        public ModerationActionSummary Action { get; }
    }
}
