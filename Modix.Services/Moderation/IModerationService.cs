using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;

namespace Modix.Services.Moderation
{
    public interface IModerationService
    {
        Task CreateInfractionAsync(InfractionType type, long subjectId, string reason, TimeSpan? duration);

        Task RescindInfractionAsync(long infractionId, string reason);

        Task<RecordsPage<InfractionSearchResult>> SearchInfractionsAsync(InfractionSearchCriteria criteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        // TODO: AsyncEventHandler?
        event EventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;
    }
}
