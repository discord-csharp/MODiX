using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using NSubstitute;

namespace Modix.Data.Test
{
    public static class TestDataContextFactory
    {
        public static ModixContext BuildTestDataContext(Action<ModixContext> initializeAction = null)
        {
            var modixContext = Substitute.ForPartsOf<ModixContext>(new DbContextOptionsBuilder<ModixContext>()
                .UseInMemoryDatabase((++_databaseName).ToString())
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                })
                //.UseLoggerFactory(new LoggerFactory(
                //    Enumerable.Empty<ILoggerProvider>()
                //        .Append(new ConsoleLoggerProvider((error, logLevel) => true, true))))
                //.EnableSensitiveDataLogging()
                .Options);

            if (!(initializeAction is null))
            {
                initializeAction.Invoke(modixContext);

                modixContext.ResetSequenceToMaxValue(
                    x => x.ClaimMappings,
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.DesignatedChannelMappings,
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.ConfigurationActions,
                    x => x.Id);

                modixContext.SaveChanges();
                modixContext.ClearReceivedCalls();
            }

            return modixContext;
        }

        private static int _databaseName;
    }
}
