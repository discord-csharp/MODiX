using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IDMChannel"/> objects.
    /// </summary>
    public static class DMChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IDMChannel"/> to an abstracted <see cref="IDMChannel"/> value.
        /// </summary>
        /// <param name="dmChannel">The existing <see cref="IDMChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="dmChannel"/>.</exception>
        /// <returns>An <see cref="IDMChannel"/> that abstracts <paramref name="dmChannel"/>.</returns>
        public static IDMChannel Abstract(this IDMChannel dmChannel)
            => dmChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(dmChannel)),
                RestDMChannel restDMChannel
                    => RestDMChannelAbstractionExtensions.Abstract(restDMChannel) as IDMChannel,
                SocketDMChannel socketDMChannel
                    => SocketDMChannelAbstractionExtensions.Abstract(socketDMChannel) as IDMChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IDMChannel)} type {dmChannel.GetType().Name}")
            };
    }
}
