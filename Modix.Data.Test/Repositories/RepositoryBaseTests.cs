using System;
using System.Reflection;

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
            {
                try
                {
                    return Substitute.For<RepositoryBase>(modixContext);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
        }

        [Test]
        public void Constructor_ModixContextIsNull_ThrowsException()
        {
            var context = new TestContext()
            {
                modixContext = null
            };

            Should.Throw<ArgumentNullException>(() =>
            {
                context.ConstructUUT();
            });
        }

        [Test]
        public void Constructor_Otherwise_ModixContextIsGiven()
        {
            var context = new TestContext();

            var uut = context.ConstructUUT();

            uut.ModixContext.ShouldBeSameAs(context.modixContext);
        }
    }
}
