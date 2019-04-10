using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="ICategoryChannel"/> objects.
    /// </summary>
    internal static class CategoryChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ICategoryChannel"/> to an abstracted <see cref="ICategoryChannel"/> value.
        /// </summary>
        /// <param name="categoryChannel">The existing <see cref="ICategoryChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="categoryChannel"/>.</exception>
        /// <returns>An <see cref="ICategoryChannel"/> that abstracts <paramref name="categoryChannel"/>.</returns>
        public static ICategoryChannel Abstract(this ICategoryChannel categoryChannel)
            => categoryChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(categoryChannel)),
                RestCategoryChannel restCategoryChannel
                    => RestCategoryChannelAbstractionExtensions.Abstract(restCategoryChannel) as ICategoryChannel,
                SocketCategoryChannel socketCategoryChannel
                    => SocketCategoryChannelAbstractionExtensions.Abstract(socketCategoryChannel) as ICategoryChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(ICategoryChannel)} type {categoryChannel.GetType().Name}")
            };
    }
}
