using System;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="DeletedMessageEntity"/>.
    /// </summary>
    public class DeletedMessageCreationData
    {
        /// <summary>
        /// See <see cref="DeletedMessageEntity.MessageId"/>.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.ChannelId"/>.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.AuthorId"/>.
        /// </summary>
        public ulong AuthorId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Content"/>
        /// </summary>
        public string Content { get; set; } = null!;

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Reason"/>
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.BatchId"/>.
        /// </summary>
        public long BatchId { get; set; }

        internal DeletedMessageEntity ToEntity()
            => new DeletedMessageEntity()
            {
                MessageId = MessageId,
                GuildId = GuildId,
                ChannelId = ChannelId,
                AuthorId = AuthorId,
                Content = Content,
                Reason = Reason,
                CreateAction = new ModerationActionEntity()
                {
                    GuildId = GuildId,
                    Type = ModerationActionType.MessageDeleted,
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = CreatedById
                }
            };

        internal DeletedMessageEntity ToBatchEntity()
            => new DeletedMessageEntity()
            {
                MessageId = MessageId,
                GuildId = GuildId,
                ChannelId = ChannelId,
                AuthorId = AuthorId,
                Content = Content,
                Reason = Reason,
                BatchId = BatchId,
            };
    }
}
