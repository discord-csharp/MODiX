using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes the identifying properties of a <see cref="DiscordUserEntity"/>.
    /// This is generally for use within other models, to refer to related users.
    /// </summary>
    public class DiscordUserIdentity
    {
        /// <summary>
        /// See <see cref="DiscordUserEntity.UserId"/>.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// See <see cref="DiscordUserEntity.Username"/>.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// See <see cref="DiscordUserEntity.Discriminator"/>.
        /// </summary>
        public uint Discriminator { get; set; }

        /// <summary>
        /// See <see cref="DiscordUserEntity.Nickname"/>.
        /// </summary>
        public string Nickname { get; set; }

        internal static Expression<Func<DiscordUserEntity, DiscordUserIdentity>> FromEntityProjection { get; }
            = entity => new DiscordUserIdentity()
            {
                UserId = entity.UserId,
                Username = entity.Username,
                Discriminator = entity.Discriminator,
                Nickname = entity.Nickname
            };
    }
}
