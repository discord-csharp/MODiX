using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class ConfigurationActionAssertions
    {
        public static void ShouldMatchTestData(this ConfigurationActionBrief brief, ulong guildId)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(ConfigurationActions.Entities.Select(x => x.Id).ToArray());

            var entity = ConfigurationActions.Entities.First(x => x.Id == brief.Id);

            brief.Type.ShouldBe(entity.Type);
            brief.Created.ShouldBe(entity.Created);
            brief.CreatedBy.ShouldMatchTestData(guildId);
        }
    }
}
