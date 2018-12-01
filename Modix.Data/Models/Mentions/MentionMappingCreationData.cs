namespace Modix.Data.Models.Mentions
{
    /// <summary>
    /// Describes an operation to create a <see cref="MentionMappingEntity"/>.
    /// </summary>
    public class MentionMappingCreationData
    {
        /// <summary>
        /// See <see cref="MentionMappingEntity.Id"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.Mentionability"/>.
        /// </summary>
        public MentionabilityType Mentionability { get; set; }

        /// <summary>
        /// See <see cref="MentionMappingEntity.MinimumRankId"/>.
        /// </summary>
        public ulong? MinimumRankId { get; set; }

        internal MentionMappingEntity ToEntity()
            => new MentionMappingEntity
            {
                GuildId = GuildId,
                RoleId = RoleId,
                Mentionability = Mentionability,
                MinimumRankId = MinimumRankId,
            };
    }
}
