using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCampaignEntity"/>, for use in higher layers of the application,
    /// which includes all comment data for the campaign.
    /// </summary>
    public class PromotionCampaignDetails
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.TargetRole"/>.
        /// </summary>
        public GuildRoleBrief TargetRole { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CreateAction"/>.
        /// </summary>
        public PromotionActionBrief CreateAction { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Outcome"/>.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CloseAction"/>.
        /// </summary>
        public PromotionActionBrief CloseAction { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Comments"/>.
        /// </summary>
        public IReadOnlyCollection<PromotionCommentCampaignBrief> Comments { get; set; }

        internal static Expression<Func<PromotionCampaignEntity, PromotionCampaignDetails>> FromEntityProjection
            = entity => new PromotionCampaignDetails()
            {
                Id = entity.Id,
                GuildId = (ulong)entity.GuildId,
                Subject = new GuildUserBrief()
                {
                    Id = (ulong)entity.Subject.UserId,
                    Username = entity.Subject.User.Username,
                    Discriminator = entity.Subject.User.Discriminator,
                    Nickname = entity.Subject.Nickname
                },
                TargetRole = new GuildRoleBrief()
                {
                    Id = (ulong)entity.TargetRole.RoleId,
                    Name = entity.TargetRole.Name
                },
                Outcome = entity.Outcome,
                CreateAction = new PromotionActionBrief()
                {
                    Id = entity.CreateAction.Id,
                    Created = entity.CreateAction.Created,
                    CreatedBy = new GuildUserBrief()
                    {
                        Id = (ulong)entity.CreateAction.CreatedBy.UserId,
                        Username = entity.CreateAction.CreatedBy.User.Username,
                        Discriminator = entity.CreateAction.CreatedBy.User.Discriminator,
                        Nickname = entity.CreateAction.CreatedBy.Nickname
                    }
                },
                CloseAction = (entity.CloseAction == null) ? null : new PromotionActionBrief()
                {
                    Id = entity.CloseAction.Id,
                    Created = entity.CloseAction.Created,
                    CreatedBy = new GuildUserBrief()
                    {
                        Id = (ulong)entity.CloseAction.CreatedBy.UserId,
                        Username = entity.CloseAction.CreatedBy.User.Username,
                        Discriminator = entity.CloseAction.CreatedBy.User.Discriminator,
                        Nickname = entity.CloseAction.CreatedBy.Nickname
                    }
                },
                Comments = entity.Comments
                    .Select(comment => new PromotionCommentCampaignBrief()
                    {
                        Id = comment.Id,
                        Sentiment = comment.Sentiment,
                        Content = comment.Content,
                        CreateAction = new PromotionActionBrief()
                        {
                            Id = comment.CreateAction.Id,
                            Created = comment.CreateAction.Created,
                            CreatedBy = new GuildUserBrief()
                            {
                                Id = (ulong)comment.CreateAction.CreatedBy.UserId,
                                Username = comment.CreateAction.CreatedBy.User.Username,
                                Discriminator = comment.CreateAction.CreatedBy.User.Discriminator,
                                Nickname = comment.CreateAction.CreatedBy.Nickname
                            }
                        }
                    })
                    .ToArray()
            };
    }
}
