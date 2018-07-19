using System;
using System.Linq.Expressions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a partial view of an <see cref="ConfigurationActionEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class ConfigurationActionBrief
    {
        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Type"/>.
        /// </summary>
        public ConfigurationActionType Type { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ConfigurationActionEntity.CreatedBy"/>.
        /// </summary>
        public UserIdentity CreatedBy { get; set; }
    }
}
