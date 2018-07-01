using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    public interface IInfractionRepository
    {
        Task InsertAsync(Infraction infraction);

        Task UpdateDurationAsync(ulong id, TimeSpan duration, ulong updatedById);

        Task UpdateIsRescindedAsync(ulong id, bool isRescinded, ulong updatedById);

        Task<ICollection<Infraction>> SearchAsync(InfractionSearchCriteria criteria);
    }
}
