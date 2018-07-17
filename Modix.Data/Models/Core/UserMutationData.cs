using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to modify a <see cref="UserEntity"/>.
    /// </summary>
    public class UserMutationData
    {
        /// <summary>
        /// See <see cref="UserEntity.Username"/>.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Username"/>.
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Username"/>.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.LastSeen"/>.
        /// </summary>
        public DateTimeOffset LastSeen { get; set; }

        internal static UserMutationData FromEntity(UserEntity entity)
            => new UserMutationData()
            {
                Username = entity.Username,
                Discriminator = entity.Discriminator,
                Nickname = entity.Nickname,
                LastSeen = entity.LastSeen,
            };

        internal void ApplyTo(UserEntity user)
        {
            user.Username = Username;
            user.Discriminator = Discriminator;
            user.Nickname = Nickname;
            user.LastSeen = LastSeen;
        }
    }
}
