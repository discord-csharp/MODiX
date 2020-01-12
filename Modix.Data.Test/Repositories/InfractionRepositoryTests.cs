using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
using Modix.Data.Models.Moderation;
using NUnit.Framework;
using NSubstitute;
using Shouldly;

using Modix.Data.Repositories;
using Modix.Data.Utilities;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class InfractionRepositoryTests
    {
        [Test]
        public void Constructor_Always_InvokesBaseConstructor()
        {
            var modixContext = Substitute.For<ModixContext>();
            var moderationActionEventHandlers = Enumerable.Empty<IModerationActionEventHandler>();
            var infractionEventHandlers = Enumerable.Empty<IInfractionEventHandler>();

            var uut = new InfractionRepository(modixContext, moderationActionEventHandlers, infractionEventHandlers);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }
    }
}
