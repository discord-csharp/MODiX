namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes an operation to modify a tag.
    /// </summary>
    public class TagMutationData
    {
        /// <summary>
        /// See <see cref="TagEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.Name"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.Uses"/>.
        /// </summary>
        public uint Uses { get; set; }

        internal static TagMutationData FromEntity(TagEntity entity)
            => new TagMutationData()
            {
                GuildId = entity.GuildId,
                Name = entity.Name,
                Content = entity.Content,
                Uses = entity.Uses,
            };

        internal TagEntity ToEntity()
            => new TagEntity()
            {
                GuildId = GuildId,
                Name = Name,
                Content = Content,
                Uses = Uses,
            };
    }
}
