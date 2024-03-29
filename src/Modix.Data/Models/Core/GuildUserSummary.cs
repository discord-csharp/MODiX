using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a <see cref="UserEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class GuildUserSummary
    {
        /// <summary>
        /// See <see cref="GuildUserEntity.Id"/>.
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// See <see cref="GuildUserEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Username"/>.
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// See <see cref="UserEntity.Discriminator"/>.
        /// </summary>
        public string Discriminator { get; set; } = null!;

        /// <summary>
        /// See <see cref="GuildUserEntity.Nickname"/>.
        /// </summary>
        public string? Nickname { get; set; }

        /// <summary>
        /// See <see cref="GuildUserEntity.FirstSeen"/>.
        /// </summary>
        public DateTimeOffset FirstSeen { get; set; }

        /// <summary>
        /// See <see cref="GuildUserEntity.LastSeen"/>.
        /// </summary>
        public DateTimeOffset LastSeen { get; set; }

        [ExpansionExpression]
        internal static readonly Expression<Func<GuildUserEntity, GuildUserSummary>> FromEntityProjection
            = entity => new GuildUserSummary()
            {
                UserId = entity.UserId,
                GuildId = entity.GuildId,
                Username = entity.User.Username,
                Discriminator = entity.User.Discriminator,
                Nickname = entity.Nickname,
                FirstSeen = entity.FirstSeen,
                LastSeen = entity.LastSeen
            };
    }
}
