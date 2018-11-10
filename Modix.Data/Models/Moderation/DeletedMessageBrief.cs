﻿using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a partial view of an <see cref="DeletedMessageEntity"/>, for use within the context of another projected model.
    /// </summary>
    public class DeletedMessageBrief
    {
        /// <summary>
        /// See <see cref="DeletedMessageEntity.MessageId"/>.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Channel"/>.
        /// </summary>
        public GuildChannelBrief Channel { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Author"/>.
        /// </summary>
        public GuildUserBrief Author { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }

        [ExpansionExpression]
        internal static Expression<Func<DeletedMessageEntity, DeletedMessageBrief>> FromEntityProjection
            = entity => new DeletedMessageBrief()
            {
                Id = (ulong)entity.MessageId,
                Channel = entity.Channel.Project(GuildChannelBrief.FromEntityProjection),
                Author = entity.Author.Project(GuildUserBrief.FromEntityProjection),
                Content = entity.Content,
                Reason = entity.Reason
            };
    }
}
