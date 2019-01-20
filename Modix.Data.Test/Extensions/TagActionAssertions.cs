using System.Linq;

using Modix.Data.Models.Tags;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    internal static class TagActionAssertions
    {
        public static void ShouldMatchTestData(this TagActionBrief brief)
        {
            brief.ShouldNotBeNull();
            brief.Id.ShouldBeOneOf(TagActions.Entities.Select(x => x.Id).ToArray());

            var entity = TagActions.Entities.First(x => x.Id == brief.Id);
            
            brief.Created.ShouldBe(entity.Created);
            brief.CreatedBy.Id.ShouldBe(entity.CreatedById);
        }

        public static void ShouldNotHaveChanged(this TagActionEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(TagActions.Entities.Select(x => x.Id).ToArray());

            var originalEntity = TagActions.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.Type.ShouldBe(originalEntity.Type);
            entity.Created.ShouldBe(originalEntity.Created);
            entity.CreatedById.ShouldBe(originalEntity.CreatedById);
            entity.NewTagId.ShouldBe(originalEntity.NewTagId);
            entity.OldTagId.ShouldBe(originalEntity.OldTagId);
        }
    }
}
