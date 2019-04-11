using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IChannel"/> objects.
    /// </summary>
    internal static class ChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IChannel"/> to an abstracted <see cref="IChannel"/> value.
        /// </summary>
        /// <param name="channel">The existing <see cref="IChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="channel"/>.</exception>
        /// <returns>An <see cref="IChannel"/> that abstracts <paramref name="channel"/>.</returns>
        public static IChannel Abstract(this IChannel channel)
            => channel switch
            {
                null
                    => throw new ArgumentNullException(nameof(channel)),
                RestChannel restChannel
                    => RestChannelAbstractionExtensions.Abstract(restChannel) as IChannel,
                SocketChannel socketChannel
                    => SocketChannelAbstractionExtensions.Abstract(socketChannel) as IChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IChannel)} type {channel.GetType().Name}")
            };
    }
}
