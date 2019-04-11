using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="ITextChannel"/> objects.
    /// </summary>
    internal static class TextChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ITextChannel"/> to an abstracted <see cref="ITextChannel"/> value.
        /// </summary>
        /// <param name="textChannel">The existing <see cref="ITextChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="textChannel"/>.</exception>
        /// <returns>An <see cref="ITextChannel"/> that abstracts <paramref name="textChannel"/>.</returns>
        public static ITextChannel Abstract(this ITextChannel textChannel)
            => textChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(textChannel)),
                RestTextChannel restTextChannel
                    => RestTextChannelAbstractionExtensions.Abstract(restTextChannel) as ITextChannel,
                SocketTextChannel socketTextChannel
                    => SocketTextChannelAbstractionExtensions.Abstract(socketTextChannel) as ITextChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(ITextChannel)} type {textChannel.GetType().Name}")
            };
    }
}
