using System;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an operation to create a <see cref="PromotionCampaignEntity"/>.
    /// </summary>
    public class PromotionCampaignCreationData
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.SubjectId"/>.
        /// </summary>
        public ulong SubjectId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.TargetRoleId"/>.
        /// </summary>
        public ulong TargetRoleId { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }

        internal PromotionCampaignEntity ToEntity()
            => new PromotionCampaignEntity()
            {
                GuildId = (long)GuildId,
                SubjectId = (long)SubjectId,
                TargetRoleId = (long)TargetRoleId,
                CreateAction = new PromotionActionEntity()
                {
                    GuildId = (long)GuildId,
                    Created = DateTimeOffset.Now,
                    Type = PromotionActionType.CampaignCreated,
                    CreatedById = (long)CreatedById
                }
            };
    }
}