using System;
using System.Collections.Generic;

namespace Discord
{
    /// <inheritdoc cref="GuildEmote" />
    public interface IGuildEmote : IEmoteEntity
    {
        /// <inheritdoc cref="GuildEmote.IsManaged" />
        bool IsManaged { get; }

        /// <inheritdoc cref="GuildEmote.RequireColons" />
        bool RequireColons { get; }

        /// <inheritdoc cref="GuildEmote.RoleIds" />
        IReadOnlyList<ulong> RoleIds { get; }

        /// <inheritdoc cref="GuildEmote.CreatorId" />
        ulong? CreatorId { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.GuildEmote"/>, through the <see cref="IGuildEmote"/> interface.
    /// </summary>
    public class GuildEmoteAbstraction : EmoteAbstraction, IGuildEmote
    {
        /// <summary>
        /// Constructs a new <see cref="GuildEmoteAbstraction"/> around an existing <see cref="Discord.GuildEmote"/>.
        /// </summary>
        /// <param name="guildEmote">The existing <see cref="Discord.GuildEmote"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildEmote"/>.</exception>
        public GuildEmoteAbstraction(GuildEmote guildEmote)
            : base(guildEmote) { }

        /// <inheritdoc />
        public bool IsManaged
            => GuildEmote.IsManaged;

        /// <inheritdoc />
        public bool RequireColons
            => GuildEmote.RequireColons;

        /// <inheritdoc />
        public IReadOnlyList<ulong> RoleIds
            => GuildEmote.RoleIds;

        /// <inheritdoc />
        public ulong? CreatorId
            => GuildEmote.CreatorId;

        /// <inheritdoc cref="GuildEmote.ToString" />
        public override string ToString()
            => GuildEmote.ToString();

        /// <summary>
        /// The existing <see cref="Discord.GuildEmote"/> being abstracted.
        /// </summary>
        internal protected GuildEmote GuildEmote
            => Emote as GuildEmote;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="GuildEmote"/> objects.
    /// </summary>
    public static class GuildEmoteAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="GuildEmote"/> to an abstracted <see cref="IGuildEmote"/> value.
        /// </summary>
        /// <param name="guildEmote">The existing <see cref="GuildEmote"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildEmote"/>.</exception>
        /// <returns>An <see cref="IGuildEmote"/> that abstracts <paramref name="guildEmote"/>.</returns>
        public static IGuildEmote Abstract(this GuildEmote guildEmote)
            => new GuildEmoteAbstraction(guildEmote);

        /// <summary>
        /// Extracts the existing <see cref="GuildEmote"/> from an abstracted <see cref="IGuildEmote"/> value.
        /// </summary>
        /// <param name="guildEmote">The <see cref="IGuildEmote"/> abstraction whose abstracted object is to be extracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="guildEmote"/>.</exception>
        /// <returns>The <see cref="GuildEmote"/> being abstracted by <paramref name="guildEmote"/>.</returns>
        internal static GuildEmote Unabstract(this IGuildEmote guildEmote)
            => guildEmote switch
            {
                null
                    => throw new ArgumentNullException(nameof(guildEmote)),
                GuildEmoteAbstraction guildEmoteAbstraction
                    => guildEmoteAbstraction.GuildEmote,
                _
                    => throw new NotSupportedException($"Unable to extract {nameof(GuildEmote)} object from {nameof(IGuildEmote)} type {guildEmote.GetType().Name}")
            };
    }
}
