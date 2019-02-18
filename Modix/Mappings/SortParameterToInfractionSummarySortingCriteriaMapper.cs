using System.Linq;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;
using Modix.Models;

namespace Modix.Mappings
{
    public static class SortParameterToInfractionSummarySortingCriteriaMapper
    {
        public static SortingCriteria[] ToInfractionSummarySortingCriteria(this SortParameter sortParameter)
        {
            var sortProperty = InfractionSummary.SortablePropertyNames.FirstOrDefault(x => x.OrdinalEquals(sortParameter.Field));

            if (sortProperty is null)
                return null;

            return new[] { new SortingCriteria()
            {
                PropertyName = sortProperty,
                Direction = sortParameter.Direction,
            }};
        }
    }
}
