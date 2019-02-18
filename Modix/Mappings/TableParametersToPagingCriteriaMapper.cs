using Modix.Data.Models;
using Modix.Models;

namespace Modix.Mappings
{
    public static class TableParametersToPagingCriteriaMapper
    {
        public static PagingCriteria ToPagingCriteria(this TableParameters tableParameters)
            => new PagingCriteria()
            {
                FirstRecordIndex = tableParameters.Page * tableParameters.PerPage,
                PageSize = tableParameters.PerPage,
            };
    }
}
