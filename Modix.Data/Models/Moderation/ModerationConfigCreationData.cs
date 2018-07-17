namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an operation to create a <see cref="ModerationConfigEntity"/>.
    /// </summary>
    public class ModerationConfigCreationData
    {
        /// <summary>
        /// See <see cref="ModerationConfigEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="ModerationConfigEntity.MuteRoleId"/>.
        /// </summary>
        public ulong MuteRoleId { get; set; }

        internal ModerationConfigEntity ToEntity()
            => new ModerationConfigEntity()
            {
                GuildId = (long)GuildId,
                MuteRoleId = (long)MuteRoleId
            };
    }
}
