using System.Linq;

using Modix.Data.Models.Tags;
using Modix.Data.Test.TestData.Tags;

namespace Shouldly
{
    internal static class TagAssertions
    {
        public static void ShouldMatchTestData(this TagSummary summary)
        {
            summary.ShouldNotBeNull();
            summary.Id.ShouldBeOneOf(Tags.Entities.Select(x => x.Id).ToArray());

            var entity = Tags.Entities.First(x => x.Id == summary.Id);

            summary.CreateAction.ShouldNotBeNull();
            summary.CreateAction.ShouldMatchTestData();

            if (!(entity.DeleteActionId is null))
            {
                summary.DeleteAction.ShouldNotBeNull();
                summary.DeleteAction.ShouldMatchTestData();
            }

            summary.GuildId.ShouldBe(entity.GuildId);
            summary.Name.ShouldBe(entity.Name);
            summary.Content.ShouldBe(entity.Content);
            summary.Uses.ShouldBe(entity.Uses);
        }

        public static void ShouldNotHaveChanged(this TagEntity entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(Tags.Entities.Select(x => x.Id).ToArray());

            var originalEntity = Tags.Entities.First(x => x.Id == entity.Id);

            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.Name.ShouldBe(originalEntity.Name);
            entity.Content.ShouldBe(originalEntity.Content);
            entity.Uses.ShouldBe(originalEntity.Uses);
            entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
            entity.DeleteActionId.ShouldBe(originalEntity.DeleteActionId);
        }
    }
}
