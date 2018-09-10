using System;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionActionSummary
    {
        /// <summary>
        /// See <see cref="PromotionActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Type"/>.
        /// </summary>
        public PromotionActionType Type { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Campaign"/>.
        /// </summary>
        public PromotionCampaignBrief Campaign { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Comment"/>.
        /// </summary>
        public PromotionCommentActionBrief Comment { get; set; }
        
        internal static Expression<Func<PromotionActionEntity, PromotionActionSummary>> FromEntityProjection
            = entity => new PromotionActionSummary()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                Created = entity.Created,
                Type = entity.Type,
                CreatedBy = new GuildUserBrief()
                {
                    Id = (ulong)entity.CreatedBy.UserId,
                    Username = entity.CreatedBy.User.Username,
                    Discriminator = entity.CreatedBy.User.Discriminator,
                    Nickname = entity.CreatedBy.Nickname
                },
                Campaign = (entity.Campaign == null) ? null : new PromotionCampaignBrief()
                {
                    Id = entity.Campaign.Id,
                    Subject = new GuildUserBrief()
                    {
                        Id = (ulong)entity.Campaign.Subject.UserId,
                        Username = entity.Campaign.Subject.User.Username,
                        Discriminator = entity.Campaign.Subject.User.Discriminator,
                        Nickname = entity.Campaign.Subject.Nickname
                    },
                    TargetRole = new GuildRoleBrief()
                    {
                        Id = (ulong)entity.Campaign.TargetRole.RoleId,
                        Name = entity.Campaign.TargetRole.Name
                    },
                    // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                    //Outcome = entity.Campaign.Outcome,
                    Outcome = (entity.Campaign.Outcome == null) ? (PromotionCampaignOutcome?)null : Enum.Parse<PromotionCampaignOutcome>(entity.Campaign.Outcome.ToString()),
                },
                Comment = (entity.Comment == null) ? null : new PromotionCommentActionBrief()
                {
                    Id = entity.Comment.Id,
                    Campaign = new PromotionCampaignBrief()
                    {
                        Id = entity.Comment.Campaign.Id,
                        Subject = new GuildUserBrief()
                        {
                            Id = (ulong)entity.Comment.Campaign.Subject.UserId,
                            Username = entity.Comment.Campaign.Subject.User.Username,
                            Discriminator = entity.Comment.Campaign.Subject.User.Discriminator,
                            Nickname = entity.Comment.Campaign.Subject.Nickname
                        },
                        TargetRole = new GuildRoleBrief()
                        {
                            Id = (ulong)entity.Comment.Campaign.TargetRole.RoleId,
                            Name = entity.Comment.Campaign.TargetRole.Name
                        },
                        // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                        //Outcome = entity.Comment.Campaign.Outcome,
                        Outcome = (entity.Comment.Campaign.Outcome == null) ? (PromotionCampaignOutcome?)null : Enum.Parse<PromotionCampaignOutcome>(entity.Comment.Campaign.Outcome.ToString()),
                    },
                    // https://github.com/aspnet/EntityFrameworkCore/issues/12834
                    //Sentiment = entity.Comment.Sentiment,
                    Sentiment = Enum.Parse<PromotionSentiment>(entity.Comment.Sentiment.ToString()),
                    Content = entity.Comment.Content
                }
            };
    }
}
