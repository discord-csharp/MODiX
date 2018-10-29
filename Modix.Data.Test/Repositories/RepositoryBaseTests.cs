using NUnit.Framework;
using NSubstitute;
using Shouldly;

using Modix.Data.Repositories;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class RepositoryBaseTests
    {
        protected class TestContext
        {
            public ModixContext modixContext = Substitute.For<ModixContext>();

            public RepositoryBase ConstructUUT()
                => Substitute.For<RepositoryBase>(modixContext);
        }

        [Test]
        public void Constructor_Always_ModixContextIsGiven()
        {
            var context = new TestContext();

            var uut = context.ConstructUUT();

            uut.ModixContext.ShouldBeSameAs(context.modixContext);
        }
    }
}
