namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="GuildRoleEntity"/>.
    /// </summary>
    public class GuildRoleCreationData
    {
        /// <summary>
        /// See <see cref="GuildRoleEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="GuildRoleEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="GuildRoleEntity.Name"/>.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// See <see cref="GuildRoleEntity.Position"/>.
        /// </summary>
        public int Position { get; set; }

        internal GuildRoleEntity ToEntity()
            => new GuildRoleEntity()
            {
                RoleId = RoleId,
                GuildId = GuildId,
                Name = Name,
                Position = Position
            };
    }
}
