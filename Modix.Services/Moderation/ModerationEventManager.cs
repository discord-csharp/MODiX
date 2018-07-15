using System;
using System.Threading.Tasks;

using AsyncEvent;

namespace Modix.Services.Moderation
{
    /// <inheritdoc />
    public class ModerationEventManager : IModerationEventManager
    {
        /// <inheritdoc />
        public event AsyncEventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;

        /// <inheritdoc />
        public async Task RaiseModerationActionCreatedAsync(Func<Task<ModerationActionCreatedEventArgs>> argsFactory)
        {
            if (argsFactory == null)
                throw new ArgumentNullException(nameof(argsFactory));

            var handler = ModerationActionCreated;
            if (handler != null)
                await ModerationActionCreated.InvokeAsync(this, await argsFactory.Invoke());
        }
    }
}
