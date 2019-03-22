using System;
using System.Threading.Tasks;
using Discord;
using Modix.Services.Messages.Modix;
using Modix.Services.NotificationDispatch;

namespace Modix.Services.AutoRemoveMessage
{
    /// <summary>
    /// Defines a service used to track removable messages.
    /// </summary>
    public interface IAutoRemoveMessageService
    {
        /// <summary>
        /// Registers a removable message with the service and adds an indicator for this to the provided embed.
        /// </summary>
        /// <param name="user">The user who can remove the message.</param>
        /// <param name="embed">The embed to operate on</param>
        /// <param name="callback">A callback that returns the <see cref="IUserMessage"/> to register as removable. The modified embed is provided with this callback.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// If the provided <paramref name="callback"/> is null.
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task RegisterRemovableMessageAsync(IUser user, EmbedBuilder embed, Func<EmbedBuilder, Task<IUserMessage>> callback);

        /// <summary>
        /// Unregisters a removable message from the service.
        /// </summary>
        /// <param name="message">The removable message.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        Task UnregisterRemovableMessageAsync(IMessage message);
    }

    /// <inheritdoc />
    internal class AutoRemoveMessageService : IAutoRemoveMessageService
    {
        private const string _footerReactMessage = "React with ❌ to remove this embed.";

        public AutoRemoveMessageService(INotificationDispatchService notificationDispatchService)
        {
            NotificationDispatchService = notificationDispatchService;
        }

        /// <inheritdoc />
        public async Task RegisterRemovableMessageAsync(IUser user, EmbedBuilder embed, Func<EmbedBuilder, Task<IUserMessage>> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (embed.Footer == null)
            {
                embed.WithFooter(_footerReactMessage);
            }
            else if (!embed.Footer.Text.Contains(_footerReactMessage))
            {
                embed.Footer.Text += $" | {_footerReactMessage}";
            }

            var msg = await callback.Invoke(embed);
            await NotificationDispatchService.PublishScopedAsync(new RemovableMessageSent()
            {
                Message = msg,
                User = user,
            });
        }

        /// <inheritdoc />
        public async Task UnregisterRemovableMessageAsync(IMessage message)
            => await NotificationDispatchService.PublishScopedAsync(new RemovableMessageRemoved()
            {
                Message = message,
            });

        protected INotificationDispatchService NotificationDispatchService { get; }
    }
}
