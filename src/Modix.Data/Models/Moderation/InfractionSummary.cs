using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a. <see cref="InfractionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class InfractionSummary
    {
        /// <summary>
        /// See <see cref="InfractionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; } = null!;

        /// <summary>
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public ModerationActionBrief CreateAction { get; set; } = null!;

        /// <summary>
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionRescinded"/>.
        /// </summary>
        public ModerationActionBrief? RescindAction { get; set; }

        /// <summary>
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionDeleted"/>.
        /// </summary>
        public ModerationActionBrief? DeleteAction { get; set; }

        /// <summary>
        /// A timestamp indicating when (if at all) this infraction expires, and should be automatically rescinded.
        /// </summary>
        public DateTimeOffset? Expires { get; set; }

        /// <summary>
        /// Defines the sortable properties of an <see cref="InfractionSummary"/>
        /// by defining the <see cref="SortingCriteria.PropertyName"/> values that are legal for use with <see cref="InfractionSummary"/> records.
        /// </summary>
        public static ICollection<string> SortablePropertyNames
            => SortablePropertyMap.Keys;

        internal static readonly IDictionary<string, Expression<Func<InfractionSummary, object?>>> SortablePropertyMap
            = new Dictionary<string, Expression<Func<InfractionSummary, object?>>>()
            {
                {
                    nameof(Id),
                    x => x.Id
                },
                {
                    nameof(Type),
                    x => x.Type
                },
                {
                    nameof(Duration),
                    x => x.Duration
                },
                {
                    "Subject",
                    x => x.Subject.Username
                },
                {
                    "Created",
                    x => x.CreateAction.Created
                },
                {
                    "Creator",
                    x => x.CreateAction.CreatedBy.Username
                },
                {
                    nameof(Expires),
                    x => x.Expires
                }
            };

        [ExpansionExpression]
        internal static readonly Expression<Func<InfractionEntity, InfractionSummary>> FromEntityProjection
            = entity => new InfractionSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Type = entity.Type,
                Reason = entity.Reason,
                Duration = entity.Duration,
                Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection),
                CreateAction = entity.CreateAction.Project(ModerationActionBrief.FromEntityProjection),
                RescindAction = (entity.RescindAction == null)
                    ? null
                    : entity.RescindAction.Project(ModerationActionBrief.FromEntityProjection),
                DeleteAction = (entity.DeleteAction == null)
                    ? null
                    : entity.DeleteAction.Project(ModerationActionBrief.FromEntityProjection),
                Expires = entity.CreateAction.Created + entity.Duration
            };
    }
}
