using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IMessage"/> objects.
    /// </summary>
    internal static class MessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IMessage"/> to an abstracted <see cref="IMessage"/> value.
        /// </summary>
        /// <param name="message">The existing <see cref="IMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="message"/>.</exception>
        /// <returns>An <see cref="IMessage"/> that abstracts <paramref name="message"/>.</returns>
        public static IMessage Abstract(this IMessage message)
            => message switch
            {
                null
                    => throw new ArgumentNullException(nameof(message)),
                RestMessage restMessage
                    => RestMessageAbstractionExtensions.Abstract(restMessage) as IMessage,
                SocketMessage socketMessage
                    => SocketMessageAbstractionExtensions.Abstract(socketMessage) as IMessage,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IMessage)} type {message.GetType().Name}")
            };
    }
}
