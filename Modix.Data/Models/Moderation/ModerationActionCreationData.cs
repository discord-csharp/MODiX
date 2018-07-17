namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="ModerationActionEntity"/>.
    /// </summary>
    public class ModerationActionCreationData
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Type"/>.
        /// </summary>
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.InfractionId"/>.
        /// </summary>
        public long? InfractionId { get; set; }

        internal ModerationActionEntity ToEntity()
            => new ModerationActionEntity()
            {
                Type = Type,
                Reason = Reason,
                CreatedById = (long)CreatedById,
                InfractionId = InfractionId
            };
    }
}
