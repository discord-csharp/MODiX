using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IUserMessage"/> objects.
    /// </summary>
    public static class UserMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IUserMessage"/> to an abstracted <see cref="IUserMessage"/> value.
        /// </summary>
        /// <param name="userMessage">The existing <see cref="IUserMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="userMessage"/>.</exception>
        /// <returns>An <see cref="IUserMessage"/> that abstracts <paramref name="userMessage"/>.</returns>
        public static IUserMessage Abstract(this IUserMessage userMessage)
            => userMessage switch
            {
                null
                    => throw new ArgumentNullException(nameof(userMessage)),
                RestUserMessage restUserMessage
                    => RestUserMessageAbstractionExtensions.Abstract(restUserMessage) as IUserMessage,
                SocketUserMessage socketUserMessage
                    => SocketUserMessageAbstractionExtensions.Abstract(socketUserMessage) as IUserMessage,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IUserMessage)} type {userMessage.GetType().Name}")
            };
    }
}
