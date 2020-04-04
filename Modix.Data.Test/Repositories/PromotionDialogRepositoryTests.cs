using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models.Promotions;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.Repositories
{
    public class PromotionDialogRepositoryTests
    {
        private static (ModixContext, PromotionDialogRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<PromotionDialogEntity>().AddRange(PromotionDialogs.Entities.Clone());
                x.Set<PromotionCampaignEntity>().AddRange(PromotionCampaigns.Entities.Clone());
            });

            var uut = new PromotionDialogRepository(modixContext);

            return (modixContext, uut);
        }

        [Test]
        public async Task GetDialogsAsync_Always_ReturnsExistingDialogs()
        {
            var (modixContext, uut) = BuildTestContext();

            var dialogs = await uut.GetDialogsAsync();

            var dialogIds = PromotionDialogs.Entities
                .Select(x => (x.MessageId, x.CampaignId))
                .ToArray();

            dialogs.Select(x => (x.MessageId, x.CampaignId))
                .ShouldBe(dialogIds, true);
        }

        [TestCaseSource(nameof(NewDialogCreationTestCases))]
        public async Task CreateAsync_Always_InsertsDialog(PromotionDialogEntity data)
        {
            var (modixContext, uut) = BuildTestContext();

            await uut.CreateAsync(data.MessageId, data.CampaignId);

            modixContext.Set<PromotionDialogEntity>().ShouldContain(
                x => x.CampaignId == data.CampaignId
                     && x.MessageId == data.MessageId);

            modixContext.Set<PromotionDialogEntity>()
                .AsEnumerable()
                .Where(x => (x.CampaignId != data.CampaignId) || (x.MessageId != data.MessageId))
                .Select(x => (x.CampaignId, x.MessageId))
                .ShouldBe(PromotionDialogs.Entities
                    .Select(x => (x.CampaignId, x.MessageId)));

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DeleteExistingDialogTestCases))]
        public async Task DeleteAsync_DialogExists_DeletesDialog_ReturnsTrue(PromotionDialogEntity data)
        {
            var (modixContext, uut) = BuildTestContext();

            (await uut.TryDeleteAsync(data.MessageId)).ShouldBeTrue();
            modixContext.Set<PromotionDialogEntity>().ShouldNotContain(
                x => x.CampaignId == data.CampaignId
                     || x.MessageId == data.MessageId);

            await modixContext.ShouldHaveReceived(1)
                .SaveChangesAsync();
        }

        [TestCaseSource(nameof(DeleteNonExistingDialogTestCases))]
        public async Task DeleteAsync_DialogDoesNotExist_ReturnsFalse(PromotionDialogEntity data)
        {
            var (modixContext, uut) = BuildTestContext();

            (await uut.TryDeleteAsync(data.MessageId)).ShouldBeFalse();

            modixContext.Set<PromotionDialogEntity>().ShouldNotContain(x => x.CampaignId == data.CampaignId);

            await modixContext.ShouldHaveReceived(0)
                .SaveChangesAsync();
        }

        private static readonly IEnumerable<TestCaseData> NewDialogCreationTestCases
            = PromotionDialogs.NewEntities
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.CampaignId}, {x.MessageId})"));

        private static readonly IEnumerable<TestCaseData> DeleteExistingDialogTestCases
            = PromotionDialogs.DeleteExistingEntities
                .Select(x => new TestCaseData(x)
                    .SetName($"{{m}}({x.CampaignId}, {x.MessageId})"));

        private static readonly IEnumerable<TestCaseData> DeleteNonExistingDialogTestCases
            = PromotionDialogs.DeleteNonExistingEntities
                .Select(x => new TestCaseData(x));
    }
}
