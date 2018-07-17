using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a summary view of a <see cref="UserEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class UserSummary
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

        /// <summary>
        /// See <see cref="UserEntity.FirstSeen"/>.
        /// </summary>
        public DateTimeOffset FirstSeen { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.LastSeen"/>.
        /// </summary>
        public DateTimeOffset LastSeen { get; set; }

        internal static readonly Expression<Func<UserEntity, UserSummary>> FromEntityProjection
            = entity => new UserSummary()
            {
                Id = (ulong)entity.Id,
                Username = entity.Username,
                Discriminator = entity.Discriminator,
                Nickname = entity.Nickname,
                FirstSeen = entity.FirstSeen,
                LastSeen = entity.LastSeen
            };
    }
}
