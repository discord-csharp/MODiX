using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class UserAssertions
    {
        public static void ShouldNotHaveChanged(this UserEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(Users.Entities.Select(x => x.Id).ToArray());

            var originalEntity = Users.Entities.First(x => x.Id == entity.Id);

            entity.Username.ShouldBe(originalEntity.Username);
            entity.Discriminator.ShouldBe(originalEntity.Discriminator);
        }
    }
}
