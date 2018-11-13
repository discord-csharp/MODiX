using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using NSubstitute;

namespace Modix.Data.Test
{
    public static class TestDataContextFactory
    {
        public static ModixContext BuildTestDataContext(string databaseName, bool useIsolatedInstance = true)
            => Substitute.ForPartsOf<ModixContext>(new DbContextOptionsBuilder<ModixContext>()
                .UseInMemoryDatabase(useIsolatedInstance
                    ? $"{databaseName}:{_random.Next().ToString()}"
                    : databaseName)
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                })
                .Options);

        private static readonly Random _random
            = new Random();
    }
}
