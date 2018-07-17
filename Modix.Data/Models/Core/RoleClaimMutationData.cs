namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to modify a <see cref="RoleClaimEntity"/>.
    /// </summary>
    public class RoleClaimMutationData
    {
        /// <summary>
        /// SEe <see cref="RoleClaimEntity.RescindActionId"/>.
        /// </summary>
        public long? RescindActionId { get; set; }

        internal static RoleClaimMutationData FromEntity(RoleClaimEntity entity)
            => new RoleClaimMutationData()
            {
                RescindActionId = entity.RescindActionId,
            };

        internal void ApplyTo(RoleClaimEntity entity)
        {
            entity.RescindActionId = RescindActionId;
        }
    }
}
