using System.Threading.Tasks;

using NUnit.Framework;
using NSubstitute;
using Shouldly;

using Modix.Data.Repositories;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class RepositoryBaseTests
    {
        [Test]
        public async Task Constructor_Always_ModixContextIsGiven()
        {
            var modixContext = await TestDataContextFactory.BuildTestDataContextAsync();

            var uut = Substitute.For<RepositoryBase>(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
    }
}
