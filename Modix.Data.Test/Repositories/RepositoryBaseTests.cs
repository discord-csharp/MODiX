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
        [Test]
        public void Constructor_ModixContextIsNull_ThrowsException()
        {
            var modixContext = null as ModixContext;

            Should.Throw<ArgumentNullException>(() =>
            {
                try
                {
                    Substitute.For<RepositoryBase>(modixContext);
                }
                catch(TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            });
        }

        [Test]
        public void Constructor_Otherwise_ModixContextIsGiven()
        {
            var modixContext = Substitute.For<ModixContext>();

            var uut = Substitute.For<RepositoryBase>(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
    }
}
