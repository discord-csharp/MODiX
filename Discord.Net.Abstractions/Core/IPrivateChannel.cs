using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IPrivateChannel"/> objects.
    /// </summary>
    public static class PrivateChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IPrivateChannel"/> to an abstracted <see cref="IPrivateChannel"/> value.
        /// </summary>
        /// <param name="privateChannel">The existing <see cref="IPrivateChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="privateChannel"/>.</exception>
        /// <returns>An <see cref="IPrivateChannel"/> that abstracts <paramref name="privateChannel"/>.</returns>
        public static IPrivateChannel Abstract(this IPrivateChannel privateChannel)
            => privateChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(privateChannel)),
                RestDMChannel restDMChannel
                    => RestDMChannelAbstractionExtensions.Abstract(restDMChannel) as IPrivateChannel,
                RestGroupChannel restGroupChannel
                    => RestGroupChannelAbstractionExtensions.Abstract(restGroupChannel) as IPrivateChannel,
                SocketDMChannel socketDMChannel
                    => SocketDMChannelAbstractionExtensions.Abstract(socketDMChannel) as IPrivateChannel,
                SocketGroupChannel socketGroupChannel
                    => SocketGroupChannelAbstractionExtensions.Abstract(socketGroupChannel) as IPrivateChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IPrivateChannel)} type {privateChannel.GetType().Name}")
            };
    }
}
