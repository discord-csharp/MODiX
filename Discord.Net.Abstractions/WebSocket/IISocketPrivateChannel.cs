using System;
using System.Collections.Generic;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="ISocketPrivateChannel" />
    public interface IISocketPrivateChannel : IPrivateChannel
    {
        /// <inheritdoc cref="ISocketPrivateChannel.Recipients" />
        new IReadOnlyCollection<ISocketUser> Recipients { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="ISocketPrivateChannel"/> objects.
    /// </summary>
    internal static class ISocketPrivateChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ISocketPrivateChannel"/> to an abstracted <see cref="IISocketPrivateChannel"/> value.
        /// </summary>
        /// <param name="iSocketPrivateChannel">The existing <see cref="ISocketPrivateChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iSocketPrivateChannel"/>.</exception>
        /// <returns>An <see cref="IISocketPrivateChannel"/> that abstracts <paramref name="iSocketPrivateChannel"/>.</returns>
        public static IISocketPrivateChannel Abstract(this ISocketPrivateChannel iSocketPrivateChannel)
            => iSocketPrivateChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(iSocketPrivateChannel)),
                SocketDMChannel socketDMChannel
                    => socketDMChannel.Abstract() as IISocketPrivateChannel,
                SocketGroupChannel socketGroupChannel
                    => socketGroupChannel.Abstract() as IISocketPrivateChannel,
                _
                    => throw new NotSupportedException($"{nameof(ISocketPrivateChannel)} type {iSocketPrivateChannel.GetType().FullName} is not supported")
            };
    }
}
