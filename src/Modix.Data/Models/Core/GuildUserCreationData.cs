using System;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="GuildUserEntity"/> and (potentially) corresponding <see cref="UserEntity"/>.
    /// </summary>
    public class GuildUserCreationData
    {
        /// <summary>
        /// See <see cref="GuildUserEntity.UserId"/>.
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

        internal UserEntity ToUserEntity()
            => new UserEntity()
            {
                Id = UserId,
                Username = Username,
                Discriminator = Discriminator
            };

        internal GuildUserEntity ToGuildDataEntity()
            => new GuildUserEntity()
            {
                UserId = UserId,
                GuildId = GuildId,
                Nickname = Nickname,
                FirstSeen = FirstSeen,
                LastSeen = LastSeen
            };
    }
}
