using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of an <see cref="DeletedMessageEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class DeletedMessageBrief
    {
        /// <summary>
        /// See <see cref="DeletedMessageEntity.MessageId"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Channel"/>.
        /// </summary>
        public GuildChannelBrief Channel { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Author"/>.
        /// </summary>
        public GuildUserIdentity Author { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }
    }
}
