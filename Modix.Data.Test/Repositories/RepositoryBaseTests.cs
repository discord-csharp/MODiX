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
        public void Constructor_Always_ModixContextIsGiven()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext();

            var uut = Substitute.For<RepositoryBase>(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
    }
}
