namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to modify an <see cref="InfractionEntity"/>.
    /// </summary>
    public class InfractionMutationData
    {
        /// <summary>
        /// See <see cref="InfractionEntity.RescindActionId"/>.
        /// </summary>
        public long? RescindActionId { get; set; }

        internal static InfractionMutationData FromEntity(InfractionEntity entity)
            => new InfractionMutationData()
            {
                RescindActionId = entity.RescindActionId
            };

        internal void ApplyTo(InfractionEntity entity)
        {
            entity.RescindActionId = RescindActionId;
        }
    }
}
