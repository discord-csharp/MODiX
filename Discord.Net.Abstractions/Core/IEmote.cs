using System;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IEmote"/> objects.
    /// </summary>
    internal static class EmoteAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IEmote"/> to an abstracted <see cref="IEmote"/> value.
        /// </summary>
        /// <param name="emote">The existing <see cref="IEmote"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emote"/>.</exception>
        /// <returns>An <see cref="IEmote"/> that abstracts <paramref name="emote"/>.</returns>
        public static IEmote Abstract(this IEmote emote)
            => emote switch
        {
            null
                => throw new ArgumentNullException(nameof(emote)),
            GuildEmote guildEmote
                => GuildEmoteAbstractionExtensions.Abstract(guildEmote) as IEmote,
            Emote emoteEntity
                => EmoteEntityAbstractionExtensions.Abstract(emoteEntity) as IEmote,
            _
                => emote // For internal Emoji class
        };
    }
}
