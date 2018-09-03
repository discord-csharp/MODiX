using System;
using System.Linq;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a set of criteria for searching for <see cref="PromotionCampaignEntity"/> entities within an <see cref="IPromotionCampaignRepository"/>.
    /// </summary>
    public class PromotionCampaignSearchCriteria
    {
        /// <summary>
        /// A <see cref="PromotionCampaignEntity.GuildId"/> value, defining the <see cref="PromotionCampaignEntity"/> entities to be returned.
        /// </summary>
        public ulong? GuildId { get; set; }

        /// <summary>
        /// A <see cref="PromotionCampaignEntity.SubjectId"/> value, defining the <see cref="PromotionCampaignEntity"/> entities to be returned.
        /// </summary>
        public ulong? SubjectId { get; set; }

        /// <summary>
        /// A <see cref="PromotionCampaignEntity.TargetRoleId"/> value, defining the <see cref="PromotionCampaignEntity"/> entities to be returned.
        /// </summary>
        public ulong? TargetRoleId { get; set; }

        /// <summary>
        /// A <see cref="PromotionCampaignEntity.Outcome"/> value, defining the <see cref="PromotionCampaignEntity"/> entities to be returned.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="PromotionCampaignEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.Created"/> value of <see cref="PromotionCampaignEntity.CreateAction"/>.
        /// </summary>
        public DateTimeOffsetRange? CreatedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="PromotionCampaignEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.CreatedById"/> value of <see cref="PromotionCampaignEntity.CreateAction"/>.
        /// </summary>
        public ulong? CreatedById { get; set; }

        /// <summary>
        /// A range of values defining the <see cref="PromotionCampaignEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.Created"/> value of <see cref="PromotionCampaignEntity.CloseAction"/>.
        /// </summary>
        public DateTimeOffsetRange? ClosedRange { get; set; }

        /// <summary>
        /// A value defining the <see cref="PromotionCampaignEntity"/> entities to be returned,
        /// according to the <see cref="PromotionActionEntity.CreatedById"/> value of <see cref="PromotionCampaignEntity.CloseAction"/>.
        /// </summary>
        public ulong? ClosedById { get; set; }

        /// <summary>
        /// A value defining the <see cref="PromotionCampaignEntity"/> entities to be returned.
        /// according to whether or not is has a <see cref="PromotionCampaignEntity.CloseActionId"/> value.
        /// </summary>
        public bool? IsClosed { get; set; }
    }

    internal static class PromotionCampaignSearchCriteriaExtensions
    {
        public static IQueryable<PromotionCampaignEntity> FilterBy(this IQueryable<PromotionCampaignEntity> query, PromotionCampaignSearchCriteria criteria)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            if (criteria is null)
                return query;

            var longGuildId = (long?)criteria.GuildId;
            var longSubjectId = (long?)criteria.SubjectId;
            var longTargetRoleId = (long?)criteria.TargetRoleId;
            var longCreatedById = (long?)criteria.CreatedById;
            var longClosedById = (long?)criteria.ClosedById;

            return query
                .FilterBy(
                    x => x.GuildId == longGuildId.Value,
                    !(longGuildId is null))
                .FilterBy(
                    x => x.SubjectId == longSubjectId.Value,
                    !(longSubjectId is null))
                .FilterBy(
                    x => x.TargetRoleId == longTargetRoleId.Value,
                    !(longTargetRoleId is null))
                .FilterBy(
                    x => x.CreateAction.Created >= criteria.CreatedRange.Value.From.Value,
                    !(criteria.CreatedRange?.From is null))
                .FilterBy(
                    x => x.CreateAction.Created <= criteria.CreatedRange.Value.To.Value,
                    !(criteria.CreatedRange?.To is null))
                .FilterBy(
                    x => x.CreateAction.CreatedById == longCreatedById.Value,
                    !(longCreatedById is null))
                .FilterBy(
                    x => x.CloseAction.Created >= criteria.ClosedRange.Value.From.Value,
                    !(criteria.ClosedRange?.From is null))
                .FilterBy(
                    x => x.CloseAction.Created <= criteria.ClosedRange.Value.To.Value,
                    !(criteria.ClosedRange?.To is null))
                .FilterBy(
                    x => (x.CloseActionId != null) && (x.CloseAction.CreatedById == longClosedById.Value),
                    !(longClosedById is null))
                .FilterBy(
                    x => x.CloseAction == null,
                    criteria.IsClosed == false)
                .FilterBy(
                    x => x.CloseAction != null,
                    criteria.IsClosed == true);
        }
    }
}
