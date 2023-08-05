using System;
using System.Collections.Immutable;
using System.Linq;
using LinqKit;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="DeletedMessageEntity"/> entities within an <see cref="IDeletedMessageRepository"/>.
    /// </summary>
    public class DeletedMessageSearchCriteria
    {
        /// <summary>
        /// A <see cref="DeletedMessageEntity.GuildId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.ChannelId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? ChannelId { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Channel.Name"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.AuthorId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? AuthorId { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Author"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.CreateAction.CreatedById"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.CreatedBy"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Content"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Reason"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.BatchId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public long? BatchId { get; set; }

        /// <summary>
        /// Defines the searchable properties of a <see cref="DeletedMessageSummary"/>.
        /// </summary>
        public static readonly ImmutableArray<string> SearchablePropertyNames = ImmutableArray.Create
            (
                nameof(Channel),
                nameof(Author),
                nameof(CreatedBy),
                nameof(Content),
                nameof(Reason),
                nameof(BatchId)
            );

        public DeletedMessageSearchCriteria WithPropertyValue(string propertyName, string propertyValue)
        {
            if (propertyName.Equals(nameof(Channel), StringComparison.OrdinalIgnoreCase))
            {
                Channel = propertyValue;
            }
            else if (propertyName.Equals(nameof(Author), StringComparison.OrdinalIgnoreCase))
            {
                Author = propertyValue;
            }
            else if (propertyName.Equals(nameof(CreatedBy), StringComparison.OrdinalIgnoreCase))
            {
                CreatedBy = propertyValue;
            }
            else if (propertyName.Equals(nameof(Content), StringComparison.OrdinalIgnoreCase))
            {
                Content = propertyValue;
            }
            else if (propertyName.Equals(nameof(Reason), StringComparison.OrdinalIgnoreCase))
            {
                Reason = propertyValue;
            }
            else if (propertyName.Equals(nameof(BatchId), StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(propertyValue, out var batchId))
                    BatchId = batchId;
            }
            else
            {
                throw new ArgumentException(nameof(propertyName));
            }

            return this;
        }
    }

    internal static class DeletedMessageQueryableExtensions
    {
        public static IQueryable<DeletedMessageEntity> FilterBy(this IQueryable<DeletedMessageEntity> query, DeletedMessageSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId,
                    criteria?.GuildId != null)
                .FilterBy(
                    x => x.ChannelId == criteria!.ChannelId,
                    criteria?.ChannelId != null)
                .FilterBy(
                    x => ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Channel.Name, criteria!.Channel!),
                    !string.IsNullOrWhiteSpace(criteria?.Channel))
                .FilterBy(
                    x => x.AuthorId == criteria!.AuthorId,
                    criteria?.AuthorId != null)
                .FilterBy(
                    x => ReusableQueries.StringContainsUser.Invoke(x.Author, criteria!.Author!),
                    !string.IsNullOrWhiteSpace(criteria?.Author))
                .FilterBy(
                    x => x.Batch == null
                        ? x.CreateAction.CreatedById == criteria!.CreatedById
                        : x.Batch.CreateAction.CreatedById == criteria!.CreatedById,
                    criteria?.CreatedById != null)
                .FilterBy(
                    x => ReusableQueries.StringContainsUser.Invoke(
                        x.Batch == null
                            ? x.CreateAction.CreatedBy!
                            : x.Batch.CreateAction.CreatedBy,
                        criteria!.CreatedBy!),
                    !string.IsNullOrWhiteSpace(criteria?.CreatedBy))
                .FilterBy(
                    x => ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Content, criteria!.Content!),
                    !string.IsNullOrWhiteSpace(criteria?.Content))
                .FilterBy(
                    x => ReusableQueries.DbCaseInsensitiveContains.Invoke(x.Reason, criteria!.Reason!),
                    !string.IsNullOrWhiteSpace(criteria?.Reason))
                .FilterBy(
                    x => x.BatchId == criteria!.BatchId,
                    criteria?.BatchId != null);
    }
}
