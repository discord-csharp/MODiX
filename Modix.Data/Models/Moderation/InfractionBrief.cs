using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of an <see cref="InfractionEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class InfractionBrief
    {
        /// <summary>
        /// See <see cref="UserEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="UserEntity.Subject"/>.
        /// </summary>
        public UserIdentity Subject { get; set; }
    }
}
