using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="GuildUserEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class GuildUserBrief
    {
        /// <summary>
        /// See <see cref="GuildUserEntity.UserId"/>.
        /// </summary>
        public ulong Id { get; set; }

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

        [ExpansionExpression]
        internal static readonly Expression<Func<GuildUserEntity, GuildUserBrief>> FromEntityProjection
            = entity => new GuildUserBrief()
            {
                Id = entity.User.Id,
                Username = entity.User.Username,
                Discriminator = entity.User.Discriminator,
                Nickname = entity.Nickname
            };
    }
}
