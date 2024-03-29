using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Shouldly;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix.Services.Test.Core
{
    [TestFixture]
    public class ChannelServiceTests
    {
        #region TrackChannelAsync() Tests

        [TestCase(1UL, "ExistingChannelName")]
        public async Task TrackChannelAsync_TryUpdateSucceeds_DoesNotCreateChannel(ulong channelId, string channelName)
        {
            var autoMocker = new AutoMocker();

            var mockCreateTransaction = new Mock<IRepositoryTransaction>();
            var mockGuildChannelRepository = autoMocker.GetMock<IGuildChannelRepository>();

            var sequence = new MockSequence();

            mockGuildChannelRepository
                .InSequence(sequence)
                .Setup(x => x.BeginCreateTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCreateTransaction.Object);

            mockGuildChannelRepository
                .InSequence(sequence)
                .Setup(x => x.TryUpdateAsync(It.IsAny<ulong>(), It.IsAny<Action<GuildChannelMutationData>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockCreateTransaction
                .InSequence(sequence)
                .Setup(x => x.Commit());

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var uut = autoMocker.CreateInstance<ChannelService>();

                var mockChannel = new Mock<IGuildChannel>();
                mockChannel
                    .Setup(x => x.Id)
                    .Returns(channelId);
                mockChannel
                    .Setup(x => x.Name)
                    .Returns(channelName);

                await uut.TrackChannelAsync(mockChannel.Object.Name, mockChannel.Object.Id, mockChannel.Object.GuildId, null, cancellationTokenSource.Token);

                mockGuildChannelRepository
                    .ShouldHaveReceived(x => x.BeginCreateTransactionAsync(cancellationTokenSource.Token), Times.Once());

                mockGuildChannelRepository
                    .ShouldHaveReceived(x => x.TryUpdateAsync(channelId, It.IsNotNull<Action<GuildChannelMutationData>>(), cancellationTokenSource.Token), Times.Once());

                mockGuildChannelRepository
                    .ShouldNotHaveReceived(x => x.CreateAsync(It.IsAny<GuildChannelCreationData>(), cancellationTokenSource.Token));

                mockCreateTransaction
                    .ShouldHaveReceived(x => x.Commit(), Times.Once());

                var updateAction = mockGuildChannelRepository
                    .Invocations
                    .First(x => x.Method.Name == nameof(IGuildChannelRepository.TryUpdateAsync))
                    .Arguments[1] as Action<GuildChannelMutationData>;

                var mutationData = new GuildChannelMutationData();
                updateAction.Invoke(mutationData);

                mutationData.Name.ShouldBe(channelName);
            }
        }

        [TestCase(1UL, 2UL, "NewChannelName")]
        public async Task TrackChannelAsync_TryUpdateFails_CreatesChannel(ulong channelId, ulong guildId, string channelName)
        {
            var autoMocker = new AutoMocker();

            var mockCreateTransaction = new Mock<IRepositoryTransaction>();
            var mockGuildChannelRepository = autoMocker.GetMock<IGuildChannelRepository>();

            var sequence = new MockSequence();

            mockGuildChannelRepository
                .InSequence(sequence)
                .Setup(x => x.BeginCreateTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCreateTransaction.Object);

            mockGuildChannelRepository
                .InSequence(sequence)
                .Setup(x => x.TryUpdateAsync(It.IsAny<ulong>(), It.IsAny<Action<GuildChannelMutationData>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            mockGuildChannelRepository
                .InSequence(sequence)
                .Setup(x => x.CreateAsync(It.IsAny<GuildChannelCreationData>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockCreateTransaction
                .InSequence(sequence)
                .Setup(x => x.Commit());

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var uut = autoMocker.CreateInstance<ChannelService>();

                var mockChannel = new Mock<IGuildChannel>();
                mockChannel
                    .Setup(x => x.Id)
                    .Returns(channelId);
                mockChannel
                    .Setup(x => x.GuildId)
                    .Returns(guildId);
                mockChannel
                    .Setup(x => x.Name)
                    .Returns(channelName);

                await uut.TrackChannelAsync(mockChannel.Object.Name, mockChannel.Object.Id, mockChannel.Object.GuildId, null, cancellationTokenSource.Token);

                mockGuildChannelRepository
                    .ShouldHaveReceived(x => x.BeginCreateTransactionAsync(cancellationTokenSource.Token), Times.Once());

                mockGuildChannelRepository
                    .ShouldHaveReceived(x => x.TryUpdateAsync(channelId, It.IsNotNull<Action<GuildChannelMutationData>>(), cancellationTokenSource.Token), Times.Once());

                mockGuildChannelRepository
                    .ShouldHaveReceived(x => x.CreateAsync(It.Is<GuildChannelCreationData>(y =>
                                (y.ChannelId == channelId)
                                && (y.GuildId == guildId)
                                && (y.Name == channelName)),
                            cancellationTokenSource.Token),
                        Times.Once());

                mockCreateTransaction
                    .ShouldHaveReceived(x => x.Commit(), Times.Once());
            }
        }

        #endregion TrackChannelAsync() Tests
    }
}
