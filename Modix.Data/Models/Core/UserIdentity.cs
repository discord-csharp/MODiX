using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes the identifying properties of a <see cref="UserEntity"/>.
    /// This is generally for use within other models, to refer to related users.
    /// </summary>
    public class UserIdentity
    {
        /// <summary>
        /// See <see cref="UserEntity.Id"/>.
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
        /// See <see cref="UserEntity.Nickname"/>.
        /// </summary>
        public string Nickname { get; set; }

        internal static readonly Expression<Func<UserEntity, UserIdentity>> FromEntityProjection
            = entity => new UserIdentity()
            {
                Id = (ulong)entity.Id,
                Username = entity.Username,
                Discriminator = entity.Discriminator,
                Nickname = entity.Nickname
            };
    }
}
