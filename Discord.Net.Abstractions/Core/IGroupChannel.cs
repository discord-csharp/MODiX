using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IGroupChannel"/> objects.
    /// </summary>
    public static class GroupChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IGroupChannel"/> to an abstracted <see cref="IGroupChannel"/> value.
        /// </summary>
        /// <param name="groupChannel">The existing <see cref="IGroupChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="groupChannel"/>.</exception>
        /// <returns>An <see cref="IGroupChannel"/> that abstracts <paramref name="groupChannel"/>.</returns>
        public static IGroupChannel Abstract(this IGroupChannel groupChannel)
            => groupChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(groupChannel)),
                RestGroupChannel restGroupChannel
                    => RestGroupChannelAbstractionExtensions.Abstract(restGroupChannel) as IGroupChannel,
                SocketGroupChannel socketGroupChannel
                    => SocketGroupChannelAbstractionExtensions.Abstract(socketGroupChannel) as IGroupChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IGroupChannel)} type {groupChannel.GetType().Name}")
            };
    }
}
