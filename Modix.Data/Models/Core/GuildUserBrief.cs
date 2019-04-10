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
        public string Username { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Discriminator"/>.
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// See <see cref="GuildUserEntity.Nickname"/>.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The name to display for this user.
        /// </summary>
        public string DisplayName
            => Nickname ?? FullUsername;

        /// <summary>
        /// The full username of the user, in <see cref="UserEntity.Username"/>#<see cref="UserEntity.Discriminator"/> format.
        /// </summary>
        public string FullUsername
            => $"{Username}#{Discriminator}";

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
