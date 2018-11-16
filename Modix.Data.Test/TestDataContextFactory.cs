using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using NSubstitute;

namespace Modix.Data.Test
{
    public static class TestDataContextFactory
    {
        public static async Task<ModixContext> BuildTestDataContextAsync(Func<ModixContext, Task> initializeAction = null)
        {
            var modixContext = Substitute.ForPartsOf<ModixContext>(new DbContextOptionsBuilder<ModixContext>()
                .UseInMemoryDatabase((++_databaseName).ToString())
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                })
                .Options);

            if (!(initializeAction is null))
            {
                await initializeAction.Invoke(modixContext);

                modixContext.SaveChanges();

                modixContext.ClearReceivedCalls();
            }

            return modixContext;
        }

        public static async Task SeedUsersAsync(this ModixContext modixContext)
            => modixContext.Users.AddRange(await TestDataFactory.BuildUsersAsync());

        public static async Task SeedGuildUsersAsync(this ModixContext modixContext)
            => modixContext.GuildUsers.AddRange(await TestDataFactory.BuildGuildUsersAsync());

        private static int _databaseName;
    }
}
