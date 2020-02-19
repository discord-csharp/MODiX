using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Data.Test.TestData;

using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test.Repositories
{
    [TestFixture]
    public class ConfigurationActionRepositoryTests
    {
        #region Test Context

        private static (ModixContext, ConfigurationActionRepository) BuildTestContext()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext(x =>
            {
                x.Set<UserEntity>().AddRange(Users.Entities.Clone());
                x.Set<GuildUserEntity>().AddRange(GuildUsers.Entities.Clone());
                x.Set<GuildRoleEntity>().AddRange(GuildRoles.Entities.Clone());
                x.Set<GuildChannelEntity>().AddRange(GuildChannels.Entities.Clone());
                x.Set<ClaimMappingEntity>().AddRange(ClaimMappings.Entities.Clone());
                x.Set<DesignatedChannelMappingEntity>().AddRange(DesignatedChannelMappings.Entities.Clone());
                x.Set<DesignatedRoleMappingEntity>().AddRange(DesignatedRoleMappings.Entities.Clone());
                x.Set<ConfigurationActionEntity>().AddRange(ConfigurationActions.Entities.Clone());
            });

            var uut = new ConfigurationActionRepository(modixContext);

            return (modixContext, uut);
        }

        #endregion Test Context

        #region Constructor() Tests

        [Test]
        public void Constructor_Always_InvokesBaseConstructor()
        {
            var modixContext = TestDataContextFactory.BuildTestDataContext();

            var uut = new ConfigurationActionRepository(modixContext);

            uut.ModixContext.ShouldBeSameAs(modixContext);
        }

        #endregion Constructor() Tests

        #region ReadAsync() Tests

        [TestCaseSource(nameof(ValidConfigurationActionIds))]
        public async Task ReadAsync_ConfigurationActionExists_ReturnsMatchingConfigurationActionSummary(long actionId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadAsync(actionId);

            result.ShouldMatchTestData();
        }

        [TestCaseSource(nameof(InvalidConfigurationActionIds))]
        public async Task ReadAsync_ConfigurationActionDoesNotExist_ReturnsNull(long actionId)
        {
            (var modixContext, var uut) = BuildTestContext();

            var result = await uut.ReadAsync(actionId);

            result.ShouldBeNull();
        }

        #endregion ReadAsync() Tests

        #region Test Data

        public static readonly IEnumerable<long> ValidConfigurationActionIds
            = ConfigurationActions.Entities
                .Select(x => x.Id);

        public static readonly IEnumerable<long> InvalidConfigurationActionIds
            = Enumerable.Empty<long>()
                .Append(ConfigurationActions.Entities.Max(x => x.Id) + 1);

        #endregion Test Data
    }
}
