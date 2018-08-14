namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to modify a <see cref="GuildChannelEntity"/> object.
    /// </summary>
    public class GuildChannelMutationData
    {
        /// <summary>
        /// See <see cref="GuildChannelEntity.Name"/>.
        /// </summary>
        public string Name { get; set; }

        internal static GuildChannelMutationData FromEntity(GuildChannelEntity entity)
            => new GuildChannelMutationData()
            {
                Name = entity.Name
            };

        internal void ApplyTo(GuildChannelEntity entity)
        {
            entity.Name = Name;
        }
    }
}
