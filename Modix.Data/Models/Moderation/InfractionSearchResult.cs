using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes an <see cref="InfractionEntity"/>, and related entities, from the perspective of a user
    /// searching through the <see cref="ModixContext.Infractions"/> dataset.
    /// </summary>
    public class InfractionSearchResult
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
        public DiscordUserIdentity Subject { get; set; }

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
        /// Defines the sortable properties of an <see cref="InfractionSearchResult"/>
        /// by defining the <see cref="SortingCriteria.PropertyName"/> values that are legal for use with <see cref="InfractionSearchResult"/> records.
        /// </summary>
        public static ICollection<string> SortablePropertyNames
            => SortablePropertyMap.Keys;

        internal static IDictionary<string, Expression<Func<InfractionSearchResult, object>>> SortablePropertyMap { get; }
            = new Dictionary<string, Expression<Func<InfractionSearchResult, object>>>()
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
                    $"{nameof(Subject)}.{nameof(InfractionSearchResult.Subject.Username)}",
                    x => x.Subject.Username
                },
                {
                    $"{nameof(Subject)}.{nameof(InfractionSearchResult.Subject.Nickname)}",
                    x => x.Subject.Nickname
                },
                {
                    $"{nameof(Subject)}.{nameof(InfractionSearchResult.Subject.Discriminator)}",
                    x => x.Subject.Discriminator
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSearchResult.CreateAction.Created)}",
                    x => x.CreateAction.Created
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy.Username)}",
                    x => x.CreateAction.CreatedBy.Username
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy.Nickname)}",
                    x => x.CreateAction.CreatedBy.Nickname
                },
                {
                    $"{nameof(CreateAction)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy)}.{nameof(InfractionSearchResult.CreateAction.CreatedBy.Discriminator)}",
                    x => x.CreateAction.CreatedBy.Discriminator
                },
            };

        internal static Expression<Func<InfractionEntity, InfractionSearchResult>> FromEntityProjection { get; }
            = entity => new InfractionSearchResult()
            {
                Id = entity.Id,
                Type = entity.Type,
                Duration = entity.Duration,
                Subject = new DiscordUserIdentity()
                {
                    UserId = entity.Subject.UserId,
                    Username = entity.Subject.Username,
                    Discriminator = entity.Subject.Discriminator,
                    Nickname = entity.Subject.Nickname
                },
                CreateAction = new ModerationActionBrief()
                {
                    Id = entity.CreateAction.Id,
                    Created = entity.CreateAction.Created,
                    CreatedBy = new DiscordUserIdentity()
                    {
                        UserId = entity.CreateAction.CreatedBy.UserId,
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
                    CreatedBy = new DiscordUserIdentity()
                    {
                        UserId = entity.RescindAction.CreatedBy.UserId,
                        Username = entity.RescindAction.CreatedBy.Username,
                        Discriminator = entity.RescindAction.CreatedBy.Discriminator,
                        Nickname = entity.RescindAction.CreatedBy.Nickname
                    },
                    Reason = entity.RescindAction.Reason
                },
                // DateTimeOffset.Now doesn't map properly, throws a NullReferenceException. Not sure if using DateTime.Now instead is problematic.
                IsExpired = (entity.Duration.HasValue && ((entity.CreateAction.Created + entity.Duration.Value) > DateTime.Now))
            };
    }
}
