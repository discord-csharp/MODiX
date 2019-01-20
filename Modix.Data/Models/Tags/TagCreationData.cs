using System;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes an operation to create a tag.
    /// </summary>
    public class TagCreationData
    {
        /// <summary>
        /// See <see cref="TagEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="TagEntity.CreateAction.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

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

        internal TagEntity ToEntity()
            => new TagEntity()
            {
                GuildId = GuildId,
                CreateAction = new TagActionEntity()
                {
                    GuildId = GuildId,
                    Created = DateTimeOffset.Now,
                    Type = TagActionType.TagCreated,
                    CreatedById = CreatedById,
                },
                Name = Name.ToLower(),
                Content = Content,
                Uses = Uses,
            };
    }
}
