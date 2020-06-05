using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

using Discord;
using Discord.WebSocket;

using Modix.Services.Core;
using Modix.Data.Repositories;
using Modix.Data.Models.Core;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class AuthorizationServiceTests
    {
        #region AutoConfigureGuildAsync() Tests

        private class AutoConfigureGuildAsyncTestContext : AutoMocker, IDisposable
        {
            public ulong SelfUserId { get; set; }
                = 1;

            public ulong GuildId { get; set; }
                = 2;

            public bool AnyClaimMappings { get; set; }
                = false;

            public IList<ulong> AdministratorRoleIds { get; }
                = new List<ulong>() { 3, 4 };

            public IList<ulong> NonAdministratorRoleIds { get; }
                = new List<ulong>() { 5, 6 };

            public IReadOnlyCollection<Mock<IRole>> MockRoles
                => _mockRoles;
            private readonly List<Mock<IRole>> _mockRoles
                = new List<Mock<IRole>>();

            public Mock<IRepositoryTransaction> MockClaimMappingCreateTransaction { get; }
                = new Mock<IRepositoryTransaction>();

            public CancellationToken CancellationToken
                => _cancellationTokenSource.Token;
            private readonly CancellationTokenSource _cancellationTokenSource
                = new CancellationTokenSource();

            public void Setup()
            {
                GetMock<IServiceProvider>()
                    .Setup(x => x.GetService(typeof(IUserService)))
                    .Returns(Get<IUserService>());

                var mockCurrentUser = GetMock<ISocketSelfUser>();
                mockCurrentUser
                    .Setup(x => x.Id)
                    .Returns(SelfUserId);

                GetMock<IDiscordSocketClient>()
                    .Setup(x => x.CurrentUser)
                    .Returns(mockCurrentUser.Object);

                GetMock<IGuild>()
                    .Setup(x => x.Id)
                    .Returns(GuildId);

                var claimMappingRepository = GetMock<IClaimMappingRepository>();
                claimMappingRepository
                    .Setup(x => x.AnyAsync(It.IsAny<ClaimMappingSearchCriteria>()))
                    .ReturnsAsync(AnyClaimMappings);
                claimMappingRepository
                    .Setup(x => x.BeginCreateTransactionAsync())
                    .ReturnsAsync(MockClaimMappingCreateTransaction.Object);

                var administratorPermission = GuildPermissions.None.Modify(administrator: true);
                var nonAdministratorPermission = GuildPermissions.All.Modify(administrator: false);

                _mockRoles.AddRange(
                    AdministratorRoleIds
                        .Select(x =>
                        {
                            var mockRole = new Mock<IRole>();
                            mockRole
                                .Setup(y => y.Id)
                                .Returns(x);
                            mockRole
                                .Setup(y => y.Permissions)
                                .Returns(administratorPermission);
                            return mockRole;
                        })
                        .Concat(NonAdministratorRoleIds
                            .Select(x =>
                            {
                                var mockRole = new Mock<IRole>();
                                mockRole
                                    .Setup(y => y.Id)
                                    .Returns(x);
                                mockRole
                                    .Setup(y => y.Permissions)
                                    .Returns(nonAdministratorPermission);
                                return mockRole;
                            })));

                GetMock<IGuild>()
                    .Setup(x => x.Roles)
                    .Returns(_mockRoles
                        .Select(x => x.Object)
                        .ToArray());
            }

            public void ClaimMappingRepositoryShouldHaveReceivedAnyAsync()
                => GetMock<IClaimMappingRepository>()
                    .ShouldHaveReceived(x => x.AnyAsync(It.Is<ClaimMappingSearchCriteria>(y =>
                            (y.Claims == null)
                            && (y.CreatedById == null)
                            && (y.CreatedRange == null)
                            && (y.GuildId == GuildId)
                            && (y.IsDeleted == false)
                            && (y.RoleIds == null))),
                        Times.Once());

            public void Dispose()
                => _cancellationTokenSource.Dispose();
        }

        [Test]
        public async Task AutoConfigureGuildAsync_GuildContainsAnyClaimMappings_DoesNotCreateClaimMappings()
        {
            var testContext = new AutoConfigureGuildAsyncTestContext()
            {
                AnyClaimMappings = true
            };

            using (testContext)
            {
                testContext.Setup();

                var uut = testContext.CreateInstance<AuthorizationService>();

                await uut.AutoConfigureGuildAsync(testContext.Get<IGuild>(), testContext.CancellationToken);

                testContext.ClaimMappingRepositoryShouldHaveReceivedAnyAsync();

                testContext.GetMock<IUserService>()
                    .Invocations.ShouldBeEmpty();

                testContext.GetMock<IClaimMappingRepository>()
                    .ShouldNotHaveReceived(x => x.BeginCreateTransactionAsync());

                testContext.GetMock<IClaimMappingRepository>()
                    .ShouldNotHaveReceived(x => x.CreateAsync(It.IsAny<ClaimMappingCreationData>()));
            }
        }

        [Test]
        public async Task AutoConfigureGuildAsync_GuildContainsNoClaimMappings_TracksSelfUser()
        {
            var testContext = new AutoConfigureGuildAsyncTestContext();

            using (testContext)
            {
                testContext.Setup();

                var uut = testContext.CreateInstance<AuthorizationService>();

                await uut.AutoConfigureGuildAsync(testContext.Get<IGuild>(), testContext.CancellationToken);

                testContext.ClaimMappingRepositoryShouldHaveReceivedAnyAsync();

                testContext.GetMock<IUserService>()
                    .ShouldHaveReceived(x => x.TrackUserAsync(testContext.Get<IGuild>(), testContext.SelfUserId), Times.Once());
            }
        }

        [Test]
        public async Task AutoConfigureGuildAsync_GuildContainsNoClaimMappings_CreatesClaimMappingsForAdministratorRoles()
        {
            var testContext = new AutoConfigureGuildAsyncTestContext();

            using (testContext)
            {
                testContext.Setup();

                var sequence = new MockSequence();

                var mockClaimMappingRepository = testContext.GetMock<IClaimMappingRepository>();

                mockClaimMappingRepository
                    .InSequence(sequence)
                    .Setup(x => x.BeginCreateTransactionAsync())
                    .ReturnsAsync(testContext.MockClaimMappingCreateTransaction.Object);

                mockClaimMappingRepository
                    .InSequence(sequence)
                    .Setup(x => x.CreateAsync(It.IsAny<ClaimMappingCreationData>()))
                    .ReturnsAsync(0);

                testContext.MockClaimMappingCreateTransaction
                    .InSequence(sequence)
                    .Setup(x => x.Commit());

                var uut = testContext.CreateInstance<AuthorizationService>();

                await uut.AutoConfigureGuildAsync(testContext.Get<IGuild>(), testContext.CancellationToken);

                testContext.ClaimMappingRepositoryShouldHaveReceivedAnyAsync();

                testContext.GetMock<IClaimMappingRepository>()
                    .ShouldHaveReceived(x => x.BeginCreateTransactionAsync(), Times.Once());

                var actualClaimMappings = testContext.GetMock<IClaimMappingRepository>()
                    .Invocations
                    .Where(x => x.Method.Name == nameof(IClaimMappingRepository.CreateAsync))
                    .Select(x => x.Arguments[0] as ClaimMappingCreationData)
                    .ToArray();

                foreach (var actualClaimMapping in actualClaimMappings)
                {
                    actualClaimMapping.CreatedById.ShouldBe(testContext.SelfUserId);
                    actualClaimMapping.GuildId.ShouldBe(testContext.GuildId);
                    actualClaimMapping.RoleId.ShouldNotBeNull();
                    actualClaimMapping.Type.ShouldBe(ClaimMappingType.Granted);
                    actualClaimMapping.UserId.ShouldBeNull();
                }

                var expectedClaimMappings = Enum.GetValues(typeof(AuthorizationClaim)).Cast<AuthorizationClaim>()
                    .SelectMany(x => testContext.AdministratorRoleIds
                        .Select(y => (claim: x, roleId: y)))
                    .ToArray();

                actualClaimMappings
                    .Select(x => (claim: x.Claim, roleId: x.RoleId.Value))
                    .ShouldBe(expectedClaimMappings);

                testContext.MockClaimMappingCreateTransaction
                    .ShouldHaveReceived(x => x.Commit(), Times.Once());
            }
        }

        #endregion AutoConfigureGuildAsync() Tests
    }
}
