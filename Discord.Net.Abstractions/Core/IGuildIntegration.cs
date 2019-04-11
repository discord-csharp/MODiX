using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IGuildIntegration"/> objects.
    /// </summary>
    internal static class GuildIntegrationAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IGuildIntegration"/> to an abstracted <see cref="IGuildIntegration"/> value.
        /// </summary>
        /// <param name="guildIntegration">The existing <see cref="IGuildIntegration"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildIntegration"/>.</exception>
        /// <returns>An <see cref="IGuildIntegration"/> that abstracts <paramref name="guildIntegration"/>.</returns>
        public static IGuildIntegration Abstract(this IGuildIntegration guildIntegration)
            => guildIntegration switch
            {
                null
                    => throw new ArgumentNullException(nameof(guildIntegration)),
                RestGuildIntegration restGuildIntegration
                    => RestGuildIntegrationAbstractionExtensions.Abstract(restGuildIntegration) as IGuildIntegration,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IGuildIntegration)} type {guildIntegration.GetType().Name}")
            };
    }
}
