namespace Modix.Data.Models.Mentions
{
    /// <summary>
    /// Describes an operation to modify a <see cref="MentionMappingEntity"/> object.
    /// </summary>
    public class MentionMappingMutationData
    {
        /// <summary>
        /// See <see cref="MentionMappingEntity.Mentionability"/>.
        /// </summary>
        public MentionabilityType Mentionability { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.MinimumRankId"/>.
        /// </summary>
        public ulong? MinimumRankId { get; set; }

        internal static MentionMappingMutationData FromEntity(MentionMappingEntity entity)
            => new MentionMappingMutationData
            {
                Mentionability = entity.Mentionability,
                MinimumRankId = entity.MinimumRankId,
            };

        internal void ApplyTo(MentionMappingEntity entity)
        {
            entity.Mentionability = Mentionability;
            entity.MinimumRankId = MinimumRankId;
        }
    }
}
