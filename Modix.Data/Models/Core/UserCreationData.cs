using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="UserEntity"/>.
    /// </summary>
    public class UserCreationData
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

        internal UserEntity ToEntity()
            => new UserEntity()
            {
                Id = (long)Id,
                Username = Username,
                Nickname = Nickname,
                Discriminator = Discriminator,
                FirstSeen = FirstSeen,
                LastSeen = LastSeen,
            };
    }
}
