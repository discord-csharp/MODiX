using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Describes a service for performing moderation actions, within the application, within the context of a single incoming request.
    /// </summary>
    public interface IModerationService
    {
        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given guild.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureGuldAsync(IGuild guild);

        /// <summary>
        /// Automatically configures role and channel permissions, related to moderation, for a given channel.
        /// </summary>
        /// <param name="channel">The channel to be configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task AutoConfigureChannelAsync(IChannel channel);

        /// <summary>
        /// Removes all moderation configuration settings for a guild, by rescinding all of its <see cref="ModerationConfigEntity"/> entries.
        /// </summary>
        /// <param name="guild">The guild to be un-configured.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has complete.</returns>
        Task UnConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Creates an infraction upon a specified user, and logs an associated moderation action.
        /// </summary>
        /// <param name="type">The value to user for <see cref="InfractionEntity.Type"/>.<</param>
        /// <param name="subjectId">The value to use for <see cref="InfractionEntity.SubjectId"/>.</param>
        /// <param name="reason">The value to use for <see cref="ModerationActionEntity.Reason"/></param>
        /// <param name="duration">The value to use for <see cref="InfractionEntity.Duration"/>.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration);

        /// <summary>
        /// Marks an existing, active, infraction of a given type, upon a given user, as rescinded.
        /// </summary>
        /// <param name="type">The <see cref="InfractionEntity.Type"/> value of the infraction to be rescinded.</param>
        /// <param name="subjectId">The <see cref="InfractionEntity.SubjectId"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(InfractionType type, ulong subjectId);

        /// <summary>
        /// Marks an existing infraction as rescinded, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task RescindInfractionAsync(long infractionId);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed.</returns>
        Task DeleteInfractionAsync(long infractionId);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria.
        /// </summary>
        /// <param name="criteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation has completed,
        /// containing the requested set of infractions.
        /// </returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria criteria, IEnumerable<SortingCriteria> sortingCriterias = null);

        /// <summary>
        /// Retrieves a collection of infractions, based on a given set of criteria, and returns a paged subset of the results, based on a given set of paging criteria.
        /// </summary>
        /// <param name="criteria">The criteria defining which infractions are to be returned.</param>
        /// <param name="sortingCriterias">The criteria defining how to sort the infractions to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation has completed, containing the requested set of infractions.</returns>
        Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria criteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);
    }
}
