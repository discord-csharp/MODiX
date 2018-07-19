using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="RoleClaimEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class RoleClaimBrief
    {
        /// <summary>
        /// See <see cref="RoleClaimEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

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
    }
}
