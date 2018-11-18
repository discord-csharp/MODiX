using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class GuildRoleAssertions
    {
        public static void ShouldNotHaveChanged(this GuildRoleEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.RoleId.ShouldBeOneOf(GuildRoles.Entities.Select(x => x.RoleId).ToArray());

            var originalEntity = GuildRoles.Entities.First(x => x.RoleId == entity.RoleId);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.Name.ShouldBe(originalEntity.Name);
            entity.Position.ShouldBe(originalEntity.Position);
        }
    }
}
