using NUnit.Framework;
using NSubstitute;
using Shouldly;

using Modix.Data.Repositories;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class ModerationActionRepositoryTests
    {
        [Test]
        public void Constructor_Always_InvokesBaseConstructor()
        {
            var modixContext = Substitute.For<ModixContext>();

            var uut = new ModerationActionRepository(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
    }
}
