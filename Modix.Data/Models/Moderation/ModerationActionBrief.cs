﻿using System;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of an <see cref="ModerationActionEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class ModerationActionBrief
    {
        /// <summary>
        /// See <see cref="ModerationActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="ModerationActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserIdentity CreatedBy { get; set; }
    }
}
