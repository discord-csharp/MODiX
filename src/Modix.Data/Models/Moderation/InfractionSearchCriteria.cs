using System;
using System.Collections.Generic;
using System.Linq;
using LinqKit;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    public class InfractionSearchCriteria
    {
        public long? Id { get; set; }
        public ulong? GuildId { get; set; }
        public InfractionType[]? Types { get; set; }
        public string? Subject { get; set; }
        public ulong? SubjectId { get; set; }
        public DateTimeOffsetRange? CreatedRange { get; set; }
        public string? Creator { get; set; }
        public ulong? CreatedById { get; set; }
        public DateTimeOffsetRange? ExpiresRange { get; set; }
        public bool? IsRescinded { get; set; }
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
