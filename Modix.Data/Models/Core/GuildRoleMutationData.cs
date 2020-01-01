namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="GuildRoleEntity"/>.
    /// </summary>
    public class GuildRoleMutationData
    {
        /// <summary>
        /// See <see cref="GuildRoleEntity.Name"/>.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// See <see cref="GuildRoleEntity.Position"/>.
        /// </summary>
        public int Position { get; set; }

        internal static GuildRoleMutationData FromEntity(GuildRoleEntity entity)
            => new GuildRoleMutationData()
            {
                Name = entity.Name,
                Position = entity.Position
            };

        internal void ApplyTo(GuildRoleEntity entity)
        {
            entity.Name = Name;
            entity.Position = Position;
        }
    }
}
