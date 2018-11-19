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

        public static void ShouldNotHaveChanged(this ConfigurationActionEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(ConfigurationActions.Entities.Select(x => x.Id).ToArray());

            var originalEntity = ConfigurationActions.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.Type.ShouldBe(originalEntity.Type);
            entity.Created.ShouldBe(originalEntity.Created);
            entity.CreatedById.ShouldBe(originalEntity.CreatedById);
            entity.ClaimMappingId.ShouldBe(originalEntity.ClaimMappingId);
            entity.DesignatedChannelMappingId.ShouldBe(originalEntity.DesignatedChannelMappingId);
            entity.DesignatedRoleMappingId.ShouldBe(originalEntity.DesignatedRoleMappingId);
        }
    }
}
