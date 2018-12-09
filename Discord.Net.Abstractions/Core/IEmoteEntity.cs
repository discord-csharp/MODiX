using System;

namespace Discord
{
    /// <inheritdoc cref="Emote" />
    public interface IEmoteEntity : IEmote, ISnowflakeEntity, IEntity<ulong>
    {
        /// <inheritdoc cref="Emote.Animated" />
        bool Animated { get; }

        /// <inheritdoc cref="Emote.Url" />
        string Url { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.Emote"/>, through the <see cref="IEmoteEntity"/> interface.
    /// </summary>
    public class EmoteAbstraction : IEmoteEntity
    {
        /// <summary>
        /// Constructs a new <see cref="EmoteAbstraction"/> around an existing <see cref="Discord.Emote"/>.
        /// </summary>
        /// <param name="emote">The existing <see cref="Discord.Emote"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emote"/>.</exception>
        public EmoteAbstraction(Emote emote)
        {
            if (emote is null)
                throw new ArgumentNullException(nameof(emote));

            Emote = emote;
        }

        /// <inheritdoc />
        public bool Animated
            => Emote.Animated;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => Emote.CreatedAt;

        /// <inheritdoc />
        public ulong Id
            => Emote.Id;

        /// <inheritdoc />
        public string Name
            => Emote.Name;

        /// <inheritdoc />
        public string Url
            => Emote.Url;

        /// <inheritdoc cref="Emote.Equals(object)" />
        public override bool Equals(object other)
            => Emote.Equals(other);

        /// <inheritdoc cref="Emote.GetHashCode" />
        public override int GetHashCode()
            => Emote.GetHashCode();

        /// <inheritdoc cref="Emote.ToString" />
        public override string ToString()
            => Emote.ToString();

        /// <summary>
        /// The existing <see cref="Discord.Emote"/> being abstracted.
        /// </summary>
        protected Emote Emote { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="Emote"/> objects.
    /// </summary>
    public static class EmoteAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="Emote"/> to an abstracted <see cref="IEmoteEntity"/> value.
        /// </summary>
        /// <param name="emote">The existing <see cref="Emote"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="emote"/>.</exception>
        /// <returns>An <see cref="IEmoteEntity"/> that abstracts <paramref name="emote"/>.</returns>
        public static IEmoteEntity Abstract(this Emote emote)
            => (emote is null) ? throw new ArgumentNullException(nameof(emote))
                : (emote is GuildEmote guildEmote) ? guildEmote.Abstract()
                : new EmoteAbstraction(emote) as IEmoteEntity;
    }
}
