using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a summary view of a <see cref="DeletedMessageEntity"/> for use in higher layers of the application.
    /// </summary>
    public class DeletedMessageSummary
    {
        /// <summary>
        /// See <see cref="DeletedMessageEntity.MessageId"/>.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Channel"/>.
        /// </summary>
        public GuildChannelBrief Channel { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Author"/>.
        /// </summary>
        public GuildUserBrief Author { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.CreateAction.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.CreateAction.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Content"/>.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Reason"/>.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.BatchId"/>.
        /// </summary>
        public long? BatchId { get; set; }

        /// <summary>
        /// See <see cref="DeletedMessageEntity.Batch"/>.
        /// </summary>
        public DeletedMessageBatchBrief Batch { get; set; }

        /// <summary>
        /// Defines the sortable properties of a <see cref="DeletedMessageSummary"/>
        /// by defining the <see cref="SortingCriteria.PropertyName"/> values that are legal for use with <see cref="DeletedMessageSummary"/> records.
        /// </summary>
        public static ICollection<string> SortablePropertyNames
            => SortablePropertyMap.Keys;

        internal static readonly IDictionary<string, Expression<Func<DeletedMessageSummary, object>>> SortablePropertyMap
            = new Dictionary<string, Expression<Func<DeletedMessageSummary, object>>>()
            {
                [$"{nameof(Channel)}.{nameof(DeletedMessageSummary.Channel.Name)}"]
                    = x => x.Channel.Name,

                [$"{nameof(Author)}.{nameof(DeletedMessageSummary.Author.Nickname)}"]
                    = x => x.Author.Nickname,

                [nameof(Created)]
                    = x => x.Created,

                [$"{nameof(CreatedBy)}.{nameof(DeletedMessageSummary.CreatedBy.Nickname)}"]
                    = x => x.CreatedBy.Nickname,

                [nameof(Content)]
                    = x => x.Content,

                [nameof(Reason)]
                    = x => x.Reason,

                [nameof(BatchId)]
                    = x => x.BatchId,
            };

        internal static readonly Expression<Func<DeletedMessageEntity, DeletedMessageSummary>> FromEntityProjection
            = entity => new DeletedMessageSummary()
            {
                MessageId = entity.MessageId,
                GuildId = entity.GuildId,
                Channel = entity.Channel.Project(GuildChannelBrief.FromEntityProjection),
                Author = entity.Author.Project(GuildUserBrief.FromEntityProjection),
                Created = entity.BatchId == null
                    ? entity.CreateAction.Created
                    : entity.Batch.CreateAction.Created,
                CreatedBy = entity.BatchId == null
                    ? entity.CreateAction.CreatedBy.Project(GuildUserBrief.FromEntityProjection)
                    : entity.Batch.CreateAction.CreatedBy.Project(GuildUserBrief.FromEntityProjection),
                Content = entity.Content,
                Reason = entity.Reason,
                BatchId = entity.BatchId,
            };
    }
}
