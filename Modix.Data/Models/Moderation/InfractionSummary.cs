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
        public string Reason { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Duration"/>.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// See <see cref="InfractionEntity.Subject"/>.
        /// </summary>
        public GuildUserIdentity Subject { get; set; }

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
        /// The associated <see cref="ModerationActionEntity"/> from <see cref="InfractionEntity.ModerationActions"/>,
        /// whose <see cref="ModerationActionEntity.Type"/> is <see cref="ModerationActionType.InfractionDeleted"/>.
        /// </summary>
        public ModerationActionBrief DeleteAction { get; set; }

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
                {
                    nameof(Expires),
                    x => x.Expires
                }
            };

        internal static Expression<Func<InfractionEntity, InfractionSummary>> FromEntityProjection { get; }
            = entity => new InfractionSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                Type = entity.Type,
                Reason = entity.Reason,
                Duration = entity.Duration,
                Subject = new GuildUserIdentity()
                {
                    Id = (ulong)entity.Subject.UserId,
                    Username = entity.Subject.User.Username,
                    Discriminator = entity.Subject.User.Discriminator,
                    Nickname = entity.Subject.Nickname
                },
                CreateAction = new ModerationActionBrief()
                {
                    Id = entity.CreateAction.Id,
                    Created = entity.CreateAction.Created,
                    CreatedBy = new GuildUserIdentity()
                    {
                        Id = (ulong)entity.CreateAction.CreatedBy.UserId,
                        Username = entity.CreateAction.CreatedBy.User.Username,
                        Discriminator = entity.CreateAction.CreatedBy.User.Discriminator,
                        Nickname = entity.CreateAction.CreatedBy.Nickname
                    }
                },
                RescindAction = (entity.RescindActionId == null) ? null : new ModerationActionBrief()
                {
                    Id = entity.RescindAction.Id,
                    Created = entity.RescindAction.Created,
                    CreatedBy = new GuildUserIdentity()
                    {
                        Id = (ulong)entity.RescindAction.CreatedBy.UserId,
                        Username = entity.RescindAction.CreatedBy.User.Username,
                        Discriminator = entity.RescindAction.CreatedBy.User.Discriminator,
                        Nickname = entity.RescindAction.CreatedBy.Nickname
                    }
                },
                DeleteAction = (entity.DeleteActionId == null) ? null : new ModerationActionBrief()
                {
                    Id = entity.DeleteAction.Id,
                    Created = entity.DeleteAction.Created,
                    CreatedBy = new GuildUserIdentity()
                    {
                        Id = (ulong)entity.DeleteAction.CreatedBy.UserId,
                        Username = entity.DeleteAction.CreatedBy.User.Username,
                        Discriminator = entity.DeleteAction.CreatedBy.User.Discriminator,
                        Nickname = entity.DeleteAction.CreatedBy.Nickname
                    }
                },
                Expires = entity.CreateAction.Created + entity.Duration
            };
    }
}
