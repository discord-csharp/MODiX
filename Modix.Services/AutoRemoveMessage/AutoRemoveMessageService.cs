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
        /// Registers a removable message with the service.
        /// </summary>
        /// <param name="message">The removable message.</param>
        /// <param name="user">The user who can remove the message.</param>
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
            if (embed.Footer != null)
            {
                embed.Footer.Text += $" | {_footerReactMessage}";
            }
            else
            {
                embed.WithFooter(_footerReactMessage);
            }

            if (callback == null)
                return;

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
