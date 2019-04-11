using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IMessageChannel"/> objects.
    /// </summary>
    internal static class MessageChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IMessageChannel"/> to an abstracted <see cref="IMessageChannel"/> value.
        /// </summary>
        /// <param name="messageChannel">The existing <see cref="IMessageChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="messageChannel"/>.</exception>
        /// <returns>An <see cref="IMessageChannel"/> that abstracts <paramref name="messageChannel"/>.</returns>
        public static IMessageChannel Abstract(this IMessageChannel messageChannel)
            => messageChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(messageChannel)),
                RestDMChannel restDMChannel
                    => RestDMChannelAbstractionExtensions.Abstract(restDMChannel) as IMessageChannel,
                RestGroupChannel restGroupChannel
                    => RestGroupChannelAbstractionExtensions.Abstract(restGroupChannel) as IMessageChannel,
                RestTextChannel restTextChannel
                    => RestTextChannelAbstractionExtensions.Abstract(restTextChannel) as IMessageChannel,
                SocketDMChannel socketDMChannel
                    => SocketDMChannelAbstractionExtensions.Abstract(socketDMChannel) as IMessageChannel,
                SocketGroupChannel socketGroupChannel
                    => SocketGroupChannelAbstractionExtensions.Abstract(socketGroupChannel) as IMessageChannel,
                SocketTextChannel socketTextChannel
                    => SocketTextChannelAbstractionExtensions.Abstract(socketTextChannel) as IMessageChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IMessageChannel)} type {messageChannel.GetType().Name}")
            };
    }
}
