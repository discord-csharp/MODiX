using System;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

namespace Modix.Services.Moderation
{
    public interface IModerationService
    {
        Task RecordInfractionAsync(InfractionTypes type, long subjectId, string reason, TimeSpan? duration);

        Task RescindInfractionAsync(long infractionId, string comment);

        Task<QueryPage<InfractionEntity>> FindInfractionsAsync(InfractionSearchCriteria criteria, PagingCriteria pagingCriteria);

        // TODO: AsyncEventHandler?
        event EventHandler<ModerationActionCreatedEventArgs> ModerationActionCreated;
    }
}
