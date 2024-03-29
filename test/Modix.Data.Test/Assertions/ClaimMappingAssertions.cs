using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Modix.Data.Models.Core;
using Modix.Data.Test.TestData;

namespace Shouldly
{
    public static class ClaimMappingAssertions
    {
        public static void ShouldMatchTestData([NotNull] this ClaimMappingBrief? summary)
        {
            summary.ShouldNotBeNull();
            summary.Id.ShouldBeOneOf(ClaimMappings.Entities.Select(x => x.Id).ToArray());

            var entity = ClaimMappings.Entities.First(x => x.Id == summary.Id);

            summary.ShouldNotBeNull();
            summary.Id.ShouldBe(entity.Id);
            summary.Type.ShouldBe(entity.Type);
            summary.RoleId.ShouldBe(entity.RoleId);
            summary.UserId.ShouldBe(entity.UserId);
            summary.Claim.ShouldBe(entity.Claim);
        }

        public static void ShouldMatchTestData([NotNull] this ClaimMappingSummary? summary)
        {
            summary.ShouldNotBeNull();
            summary.Id.ShouldBeOneOf(ClaimMappings.Entities.Select(x => x.Id).ToArray());
            
            var entity = ClaimMappings.Entities.First(x => x.Id == summary.Id);

            summary.ShouldNotBeNull();
            summary.Id.ShouldBe(entity.Id);
            summary.Type.ShouldBe(entity.Type);
            summary.GuildId.ShouldBe(entity.GuildId);
            summary.RoleId.ShouldBe(entity.RoleId);
            summary.UserId.ShouldBe(entity.UserId);
            summary.Claim.ShouldBe(entity.Claim);
            summary.CreateAction.ShouldMatchTestData(entity.GuildId);
            if (entity.DeleteActionId is null)
                summary.DeleteAction.ShouldBeNull();
            else
                summary.DeleteAction!.ShouldMatchTestData(entity.GuildId);
        }

        public static void ShouldNotHaveChanged([NotNull] this ClaimMappingEntity? entity)
        {
            entity.ShouldNotBeNull();
            entity.Id.ShouldBeOneOf(ClaimMappings.Entities.Select(x => x.Id).ToArray());

            var originalEntity = ClaimMappings.Entities.First(x => x.Id == entity.Id);

            entity.ShouldNotBeNull();
            entity.Id.ShouldBe(originalEntity.Id);
            entity.Type.ShouldBe(originalEntity.Type);
            entity.GuildId.ShouldBe(originalEntity.GuildId);
            entity.RoleId.ShouldBe(originalEntity.RoleId);
            entity.UserId.ShouldBe(originalEntity.UserId);
            entity.Claim.ShouldBe(originalEntity.Claim);
            entity.CreateActionId.ShouldBe(originalEntity.CreateActionId);
            entity.DeleteActionId.ShouldBe(originalEntity.DeleteActionId);
        }
    }
}
