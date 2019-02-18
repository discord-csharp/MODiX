using System.Linq;

using Modix.Data.Utilities;
using Modix.Models;

namespace Modix.Extensions
{
    public static class FilterParameterExtensions
    {
        public static  string GetFieldValue(this FilterParameter[] filterParameters, string propertyName)
            => filterParameters.FirstOrDefault(x => x.Field.OrdinalEquals(propertyName))?.Value;
    }
}
