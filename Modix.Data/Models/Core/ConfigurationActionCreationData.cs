namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create a <see cref="ConfigurationActionEntity"/>.
    /// </summary>
    public class ConfigurationActionCreationData
    {
        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Type"/>.
        /// </summary>
        public ConfigurationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.RoleClaimId"/>.
        /// </summary>
        public long? RoleClaimId { get; set; }

        internal ConfigurationActionEntity ToEntity()
            => new ConfigurationActionEntity()
            {
                Type = Type,
                CreatedById = (long)CreatedById,
                RoleClaimId = RoleClaimId
            };
    }
}
