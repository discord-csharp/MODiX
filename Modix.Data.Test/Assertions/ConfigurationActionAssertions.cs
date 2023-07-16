using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class ConfigurationActionAssertions
    {
        public static void ShouldMatchTestData([NotNull] this ConfigurationActionBrief? brief, ulong guildId)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(ConfigurationActions.Entities.Select(x => x.Id).ToArray());

            var entity = ConfigurationActions.Entities.First(x => x.Id == brief.Id);

            brief.Type.ShouldBe(entity.Type);
            brief.Created.ShouldBe(entity.Created);
            brief.CreatedBy.ShouldMatchTestData(guildId);
        }

        public static void ShouldMatchTestData([NotNull] this ConfigurationActionSummary? summary)
        {
            summary.ShouldNotBeNull();
            summary.Id.ShouldBeOneOf(ConfigurationActions.Entities.Select(x => x.Id).ToArray());

            var entity = ConfigurationActions.Entities.First(x => x.Id == summary.Id);

            summary.GuildId.ShouldBe(entity.GuildId);
            summary.Type.ShouldBe(entity.Type);
            summary.Created.ShouldBe(entity.Created);
            summary.CreatedBy.ShouldMatchTestData(summary.GuildId);

            if (entity.ClaimMappingId is null)
                summary.ClaimMapping.ShouldBeNull();
            else
                summary.ClaimMapping!.ShouldMatchTestData();

            if (entity.DesignatedChannelMappingId is null)
                summary.DesignatedChannelMapping.ShouldBeNull();
            else
                summary.DesignatedChannelMapping!.ShouldMatchTestData();

            if (entity.DesignatedRoleMappingId is null)
                summary.DesignatedRoleMapping.ShouldBeNull();
            else
                summary.DesignatedRoleMapping!.ShouldMatchTestData();
        }

        public static void ShouldNotHaveChanged([NotNull] this ConfigurationActionEntity? entity)
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
