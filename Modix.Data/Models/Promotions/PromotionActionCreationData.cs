using System;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an operation to create a <see cref="PromotionActionEntity"/>.
    /// </summary>
    public class PromotionActionCreationData
    {
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
        /// See <see cref="PromotionActionEntity.CreatedById"/>.
        /// </summary>
        public ulong CreatedById { get; set; }
    }
}
