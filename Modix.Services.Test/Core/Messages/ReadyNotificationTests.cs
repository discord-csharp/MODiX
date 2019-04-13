using NUnit.Framework;
using Shouldly;

using Discord;

namespace Modix.Services.Test.Core.Messages
{
    [TestFixture]
    public class ReadyNotificationTests
    {
        #region Default Tests

        [Test]
        public void Default_Always_IsNotNull()
        {
            ReadyNotification.Default.ShouldNotBeNull();
        }

        #endregion Default Tests
    }
}
