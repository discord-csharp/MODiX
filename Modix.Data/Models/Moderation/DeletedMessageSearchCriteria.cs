using System.Linq;

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
        /// A <see cref="DeletedMessageEntity.AuthorId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? AuthorId { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.CreateAction.CreatedById"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Content"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.Reason"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// A <see cref="DeletedMessageEntity.BatchId"/> value, defining the <see cref="DeletedMessageEntity"/> entities to be returned.
        /// </summary>
        public long? BatchId { get; set; }
    }

    internal static class DeletedMessageQueryableExtensions
    {
        public static IQueryable<DeletedMessageEntity> FilterBy(this IQueryable<DeletedMessageEntity> query, DeletedMessageSearchCriteria criteria)
            => query
                .FilterBy(
                    x => x.GuildId == criteria.GuildId,
                    criteria?.GuildId != null)
                .FilterBy(
                    x => x.ChannelId == criteria.ChannelId,
                    criteria?.ChannelId != null)
                .FilterBy(
                    x => x.AuthorId == criteria.AuthorId,
                    criteria?.AuthorId != null)
                .FilterBy(
                    x => x.CreateAction.CreatedById == criteria.CreatedById,
                    criteria?.CreatedById != null)
                .FilterBy(
                    x => x.Content.OrdinalContains(criteria.Content),
                    !string.IsNullOrWhiteSpace(criteria?.Content))
                .FilterBy(
                    x => x.Reason.OrdinalContains(criteria.Reason),
                    !string.IsNullOrWhiteSpace(criteria?.Reason))
                .FilterBy(
                    x => x.BatchId == criteria.BatchId,
                    criteria?.BatchId != null);
    }
}
