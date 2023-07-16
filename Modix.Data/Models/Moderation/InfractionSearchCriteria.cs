using System;
using System.Collections.Generic;
using System.Linq;
using LinqKit;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="InfractionEntity"/> entities within an <see cref="IInfractionRepository"/>.
    /// </summary>
    public class InfractionSearchCriteria
    {
        /// <summary>
        /// A <see cref="InfractionEntity.Id"/> value, defining the <see cref="InfractionEntity"/> entity to be returned.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.GuildId"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A set of <see cref="InfractionEntity.Type"/> values, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public InfractionType[]? Types { get; set; }

        /// <summary>
        /// A <see cref="GuildUserBrief.DisplayName"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// A <see cref="InfractionEntity.SubjectId"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public ulong? SubjectId { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="InfractionEntity"/> entities to be returned,
        /// according to the <see cref="ModerationActionEntity.Created"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A <see cref="GuildUserBrief.DisplayName"/> value, defining the <see cref="InfractionEntity"/> entities to be returned.
        /// </summary>
        public string? Creator { get; set; }

        /// <summary>
        /// A value defining the <see cref="InfractionEntity"/> entities to be returned.
        /// according to the <see cref="ModerationActionEntity.CreatedById"/> value of associated <see cref="ModerationActionEntity"/> entities,
        /// with a <see cref="ModerationActionEntity.Type"/> value of <see cref="ModerationActionType.InfractionCreated"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="InfractionEntity"/> entities to be returned,
        /// according to the value of <see cref="InfractionEntity.Duration"/> and the <see cref="ModerationActionEntity.Created"/> value
        /// of <see cref="InfractionEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? ExpiresRange { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.RescindActionId"/> value of null,
        /// or non-null, (or both).
        /// </summary>
        public bool? IsRescinded { get; set; }

        /// <summary>
        /// A flag indicating whether records to be returned should have an <see cref="InfractionEntity.DeleteActionId"/> value of null,
        /// or non-null, (or both).
        /// </summary>
        public bool? IsDeleted { get; set; }
    }

    internal static class InfractionQueryableExtensions
    {
        public static IQueryable<InfractionEntity> FilterBy(this IQueryable<InfractionEntity> query, InfractionSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.Id == criteria.Id,
                    criteria?.Id != null)
                .FilterBy(
                    x => x.GuildId == criteria!.GuildId,
                    criteria?.GuildId != null)
                .FilterBy(
                    x => criteria!.Types!.Contains(x.Type),
                    criteria?.Types?.Any() ?? false)
                .FilterBy(
                    x => ReusableQueries.StringContainsUser.Invoke(x.Subject, criteria!.Subject!),
                    !string.IsNullOrWhiteSpace(criteria?.Subject))
                .FilterBy(
                    x => x.SubjectId == criteria!.SubjectId,
                    criteria?.SubjectId != null)
                .FilterBy(
                    x => x.CreateAction.Created >= criteria!.CreatedRange!.Value.From,
                    criteria?.CreatedRange?.From != null)
                .FilterBy(
                    x => x.CreateAction.Created <= criteria!.CreatedRange!.Value.To,
                    criteria?.CreatedRange?.To != null)
                .FilterBy(
                    x => ReusableQueries.StringContainsUser.Invoke(x.CreateAction.CreatedBy, criteria!.Creator!),
                    !string.IsNullOrWhiteSpace(criteria?.Creator))
                .FilterBy(
                    x => x.CreateAction.CreatedById == criteria!.CreatedById,
                    criteria?.CreatedById != null)
                .FilterBy(
                    x => (x.RescindActionId != null) == criteria!.IsRescinded,
                    criteria?.IsRescinded != null)
                .FilterBy(
                    x => (x.DeleteActionId != null) == criteria!.IsDeleted,
                    criteria?.IsDeleted != null)
                .FilterBy(
                    x => (x.CreateAction.Created + x.Duration) >= criteria!.ExpiresRange!.Value.From,
                    criteria?.ExpiresRange?.From != null)
                .FilterBy(
                    x => (x.CreateAction.Created + x.Duration) <= criteria!.ExpiresRange!.Value.To,
                    criteria?.ExpiresRange?.To != null);
    }
}
