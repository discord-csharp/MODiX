using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
        /// See <see cref="InfractionEntity.Type"/>.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Subject"/>.
        /// </summary>
        public UserIdentity Subject { get; set; }

        /// <summary>
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public ModerationActionBrief CreateAction { get; set; }

        /// <summary>
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionRescinded"/>.
        /// </summary>
        public ModerationActionBrief RescindAction { get; set; }

        /// <summary>
        /// A flag indicating whether the infraction has expired, as defined by <see cref="Created"/>, <see cref="Duration"/>,
        /// and the current system time.
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// Defines the sortable properties of an <see cref="InfractionSummary"/>
        /// by defining the <see cref="SortingCriteria.PropertyName"/> values that are legal for use with <see cref="InfractionSummary"/> records.
        /// </summary>
        public static ICollection<string> SortablePropertyNames
            => SortablePropertyMap.Keys;

        internal static IDictionary<string, Expression<Func<InfractionSummary, object>>> SortablePropertyMap { get; }
            = new Dictionary<string, Expression<Func<InfractionSummary, object>>>()
            {
                {
                    nameof(Type),
                    x => x.Type
                },
                {
                    nameof(Duration),
                    x => x.Duration
                },
                {
                    $"{nameof(Subject)}.{nameof(InfractionSummary.Subject.Username)}",
                    x => x.Subject.Username
                },
                {
                    $"{nameof(Subject)}.{nameof(InfractionSummary.Subject.Nickname)}",
                    x => x.Subject.Nickname
                },
                {
                    $"{nameof(Subject)}.{nameof(InfractionSummary.Subject.Discriminator)}",
                    x => x.Subject.Discriminator
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSummary.CreateAction.Created)}",
                    x => x.CreateAction.Created
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSummary.CreateAction.CreatedBy)}.{nameof(InfractionSummary.CreateAction.CreatedBy.Username)}",
                    x => x.CreateAction.CreatedBy.Username
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSummary.CreateAction.CreatedBy)}.{nameof(InfractionSummary.CreateAction.CreatedBy.Nickname)}",
                    x => x.CreateAction.CreatedBy.Nickname
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSummary.CreateAction.CreatedBy)}.{nameof(InfractionSummary.CreateAction.CreatedBy.Discriminator)}",
                    x => x.CreateAction.CreatedBy.Discriminator
                },
            };

        internal static Expression<Func<InfractionEntity, InfractionSummary>> FromEntityProjection { get; }
            = entity => new InfractionSummary()
            {
                Id = entity.Id,
                Type = entity.Type,
                Duration = entity.Duration,
                Subject = new UserIdentity()
                {
                    Id = (ulong)entity.Subject.Id,
                    Username = entity.Subject.Username,
                    Discriminator = entity.Subject.Discriminator,
                    Nickname = entity.Subject.Nickname
                },
                CreateAction = new ModerationActionBrief()
                {
                    Id = entity.CreateAction.Id,
                    Created = entity.CreateAction.Created,
                    CreatedBy = new UserIdentity()
                    {
                        Id = (ulong)entity.CreateAction.CreatedBy.Id,
                        Username = entity.CreateAction.CreatedBy.Username,
                        Discriminator = entity.CreateAction.CreatedBy.Discriminator,
                        Nickname = entity.CreateAction.CreatedBy.Nickname
                    },
                    Reason = entity.CreateAction.Reason
                },
                RescindAction = new ModerationActionBrief()
                {
                    Id = entity.RescindAction.Id,
                    Created = entity.RescindAction.Created,
                    CreatedBy = new UserIdentity()
                    {
                        Id = (ulong)entity.RescindAction.CreatedBy.Id,
                        Username = entity.RescindAction.CreatedBy.Username,
                        Discriminator = entity.RescindAction.CreatedBy.Discriminator,
                        Nickname = entity.RescindAction.CreatedBy.Nickname
                    },
                    Reason = entity.RescindAction.Reason
                },
                IsExpired = (entity.Duration.HasValue && ((entity.CreateAction.Created + entity.Duration.Value) > DateTime.Now))
            };
    }
}
