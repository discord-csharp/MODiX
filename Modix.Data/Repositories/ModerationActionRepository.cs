using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    public interface IModerationActionRepository
    {
        Task InsertAsync(ModerationAction action);

        Task<ICollection<ModerationAction>> SearchAsync(ModerationActionSearchCriteria criteria);
    }
}
