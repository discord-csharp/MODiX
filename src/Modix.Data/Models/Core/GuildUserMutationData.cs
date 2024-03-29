using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to modify a pair of <see cref="UserEntity"/> and <see cref="GuildUserEntity"/> objects.
    /// </summary>
    public class GuildUserMutationData
    {
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
        /// See <see cref="GuildUserEntity.LastSeen"/>.
        /// </summary>
        public DateTimeOffset LastSeen { get; set; }

        internal static GuildUserMutationData FromEntity(GuildUserEntity entity)
            => new GuildUserMutationData()
            {
                Username = entity.User.Username,
                Discriminator = entity.User.Discriminator,
                Nickname = entity.Nickname,
                LastSeen = entity.LastSeen
            };

        internal void ApplyTo(GuildUserEntity entity)
        {
            entity.User.Username = Username;
            entity.User.Discriminator = Discriminator;
            entity.Nickname = Nickname;
            entity.LastSeen = LastSeen;
        }
    }
}
