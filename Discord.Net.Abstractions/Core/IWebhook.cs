using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IWebhook"/> objects.
    /// </summary>
    public static class WebhookAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IWebhook"/> to an abstracted <see cref="IWebhook"/> value.
        /// </summary>
        /// <param name="webhook">The existing <see cref="IWebhook"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="webhook"/>.</exception>
        /// <returns>An <see cref="IWebhook"/> that abstracts <paramref name="webhook"/>.</returns>
        public static IWebhook Abstract(this IWebhook webhook)
            => webhook switch
            {
                null
                    => throw new ArgumentNullException(nameof(webhook)),
                RestWebhook restWebhook
                    => RestWebhookAbstractionExtensions.Abstract(restWebhook) as IWebhook,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IWebhook)} type {webhook.GetType().Name}")
            };
    }
}
