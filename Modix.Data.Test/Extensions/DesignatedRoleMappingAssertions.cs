using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class DesignatedRoleMappingAssertions
    {
        public static void ShouldMatchTestData(this DesignatedRoleMappingBrief brief)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(DesignatedRoleMappings.Entities.Select(x => x.Id).ToArray());

            var entity = DesignatedRoleMappings.Entities.First(x => x.Id == brief.Id);

            brief.Role.ShouldMatchTestData();
            brief.Type.ShouldBe(entity.Type);
        }

        public static void ShouldNotHaveChanged(this DesignatedRoleMappingEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(DesignatedRoleMappings.Entities.Select(x => x.Id).ToArray());

            var originalEntity = DesignatedRoleMappings.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.RoleId.ShouldBe(originalEntity.RoleId);
            entity.Type.ShouldBe(originalEntity.Type);
            entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
            entity.DeleteActionId.ShouldBe(originalEntity.DeleteActionId);
        }
    }
}
