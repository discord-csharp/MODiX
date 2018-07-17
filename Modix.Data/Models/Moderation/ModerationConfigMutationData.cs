namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to modify a <see cref="ModerationConfigEntity"/>.
    /// </summary>
    public class ModerationConfigMutationData
    {
        /// <summary>
        /// See <see cref="ModerationConfigEntity.MuteRoleId"/>.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        internal static ModerationConfigMutationData FromEntity(ModerationConfigEntity entity)
            => new ModerationConfigMutationData()
            {
                MuteRoleId = (ulong)entity.MuteRoleId
            };

        internal void ApplyTo(ModerationConfigEntity entity)
        {
            entity.MuteRoleId = (long)MuteRoleId;
        }
    }
}
