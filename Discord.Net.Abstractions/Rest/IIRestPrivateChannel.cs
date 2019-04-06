using System;
using System.Collections.Generic;

namespace Discord.Rest
{
    /// <inheritdoc cref="IRestPrivateChannel" />
    public interface IIRestPrivateChannel : IPrivateChannel
    {
        /// <inheritdoc cref="IIRestPrivateChannel" />
        new IReadOnlyCollection<IRestUser> Recipients { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="IRestPrivateChannel"/> objects.
    /// </summary>
    public static class IRestPrivateChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IRestPrivateChannel"/> to an abstracted <see cref="IIRestPrivateChannel"/> value.
        /// </summary>
        /// <param name="iRestPrivateChannel">The existing <see cref="IRestPrivateChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="iRestPrivateChannel"/>.</exception>
        /// <returns>An <see cref="IIRestPrivateChannel"/> that abstracts <paramref name="iRestPrivateChannel"/>.</returns>
        public static IIRestPrivateChannel Abstract(this IRestPrivateChannel iRestPrivateChannel)
            => (iRestPrivateChannel is null) ? throw new ArgumentNullException(nameof(iRestPrivateChannel))
                : (iRestPrivateChannel is RestDMChannel restDMChannel) ? restDMChannel.Abstract() as IIRestPrivateChannel
                : (iRestPrivateChannel is RestGroupChannel restGroupChannel) ? restGroupChannel.Abstract() as IIRestPrivateChannel
                : throw new NotSupportedException($"{nameof(IRestPrivateChannel)} type {iRestPrivateChannel.GetType().FullName} is not supported");
    }
}
