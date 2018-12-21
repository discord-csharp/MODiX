using Modix.Data.Models.Moderation;
using Modix.Extensions;
using Modix.Models;

namespace Modix.Mappings
{
    public static class FilterParametersToInfractionSearchCriteriaMapper
    {
        public static InfractionSearchCriteria ToInfractionSearchCriteria(this FilterParameter[] filterParameters)
        {
            return new InfractionSearchCriteria()
            {
                Id = filterParameters.GetFieldValue("id")?.ToLong(),
                Types = filterParameters.GetFieldValue("type")?.ToInfractionTypes(),
                Subject = filterParameters.GetFieldValue("subject")?.ToStringIfNotUlong(),
                SubjectId = filterParameters.GetFieldValue("subject")?.ToUlong(),
                Creator = filterParameters.GetFieldValue("creator")?.ToStringIfNotUlong(),
                CreatedById = filterParameters.GetFieldValue("creator")?.ToUlong(),
            };
        }
    }
}
