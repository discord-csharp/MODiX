namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to modify a <see cref="ConfigurationActionEntity"/>.
    /// </summary>
    public class ConfigurationActionMutationData
    {
        /// <summary>
        /// See <see cref="ConfigurationActionEntity.RoleClaimId"/>.
        /// </summary>
        public long? RoleClaimId { get; set; }

        internal static ConfigurationActionMutationData FromEntity(ConfigurationActionEntity entity)
            => new ConfigurationActionMutationData()
            {
                RoleClaimId = entity.RoleClaimId
            };

        internal void ApplyTo(ConfigurationActionEntity entity)
        {
            entity.RoleClaimId = RoleClaimId;
        }
    }
}
