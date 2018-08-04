namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes the identifying properties of a <see cref="UserEntity"/>.
    /// This is generally for use within other models, to refer to related users.
    /// </summary>
    public class GuildUserIdentity
    {
        /// <summary>
        /// See <see cref="UserEntity.Id"/>.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// See <see cref="GuildUserEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

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
            => Nickname ?? $"{Username}#{Discriminator}";
    }
}
