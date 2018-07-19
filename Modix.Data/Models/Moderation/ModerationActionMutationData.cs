namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to modify a <see cref="ModerationActionEntity"/>.
    /// </summary>
    public class ModerationActionMutationData
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.InfractionId"/>.
        /// </summary>
        public long? InfractionId { get; set; }

        internal static ModerationActionMutationData FromEntity(ModerationActionEntity entity)
            => new ModerationActionMutationData()
            {
                InfractionId = entity.InfractionId
            };

        internal void ApplyTo(ModerationActionEntity entity)
        {
            entity.InfractionId = InfractionId;
        }
    }
}
