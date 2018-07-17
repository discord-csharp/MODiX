namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an operation to create an <see cref="RoleClaimEntity"/>.
    /// </summary>
    public class RoleClaimCreationData
    {
        /// <summary>
        /// See <see cref="RoleClaimEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.RoleId"/>.
        /// </summary>
        public ulong RoleId { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.Claim"/>.
        /// </summary>
        public AuthorizationClaim Claim { get; set; }

        /// <summary>
        /// See <see cref="RoleClaimEntity.CreateActionId"/>.
        /// </summary>
        public long CreateActionId { get; set; }

        internal RoleClaimEntity ToEntity()
            => new RoleClaimEntity()
            {
                GuildId = (long)GuildId,
                RoleId = (long)RoleId,
                Claim = Claim,
                CreateActionId = CreateActionId
            };
    }
}
