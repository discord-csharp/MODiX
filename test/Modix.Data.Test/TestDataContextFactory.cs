using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using NSubstitute;

using Modix.Data.Models.Core;
using Modix.Data.Models.Tags;

namespace Modix.Data.Test
{
    public static class TestDataContextFactory
    {
        public static ModixContext BuildTestDataContext(Action<ModixContext>? initializeAction = null)
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
                initializeAction.Invoke(modixContext);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<ClaimMappingEntity>(),
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<DesignatedChannelMappingEntity>(),
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<DesignatedRoleMappingEntity>(),
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<ConfigurationActionEntity>(),
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<TagEntity>(),
                    x => x.Id);

                modixContext.ResetSequenceToMaxValue(
                    x => x.Set<TagActionEntity>(),
                    x => x.Id);

                modixContext.SaveChanges();
                modixContext.ClearReceivedCalls();
            }

            return modixContext;
        }

        private static int _databaseName;
    }
}
