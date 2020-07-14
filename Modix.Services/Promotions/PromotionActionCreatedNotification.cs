using System;

using Modix.Data.Models.Promotions;

namespace Modix.Services.Promotions
{
    /// <summary>
    /// Describes an application-wide notification that occurs when a new <see cref="PromotionActionEntity"/> is created.-
    /// </summary>
    public class PromotionActionCreatedNotification
    {
        /// <summary>
        /// Constructs a new <see cref="PromotionActionCreatedNotification"/> from the given values.
        /// </summary>
        /// <param name="id">The value to use for <see cref="Id"/>.</param>
        /// <param name="data">The value to use for <see cref="Data"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        public PromotionActionCreatedNotification(long id, PromotionActionCreationData data)
        {
            Id = id;
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// The <see cref="PromotionActionEntity.Id"/> value of the <see cref="PromotionActionEntity"/> that was created.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// The set of data that was used to create the <see cref="PromotionActionEntity"/>.
        /// </summary>
        public PromotionActionCreationData Data { get; }
    }
}
