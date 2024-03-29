using System;
using System.Collections.Generic;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an object that represents the creation of a new <see cref="DeletedMessageBatchEntity"/>.
    /// </summary>
    public class DeletedMessageBatchCreationData
    {
        /// <summary>
        /// See <see cref="DeletedMessageBatchEntity.DeletedMessages"/>.
        /// </summary>
        public IEnumerable<DeletedMessageCreationData> Data { get; set; } = null!;

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }
        
        internal DeletedMessageBatchEntity ToEntity()
            => new DeletedMessageBatchEntity()
            {
                CreateAction = new ModerationActionEntity()
                {
                    Created = DateTimeOffset.UtcNow,
                    CreatedById = CreatedById,
                    GuildId = GuildId,
                    Type = ModerationActionType.MessageBatchDeleted,
                }
            };
    }
}
