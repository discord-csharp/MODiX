using System;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;
using Modix.Services.Moderation;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace Modix.Services.Test
{
    public class MediatrIntegrationTests
    {
        [Test]
        public async Task MediatrCanResolveNotificationHandlers()
        {
            var autoMocker = new AutoMocker();
            var sut = autoMocker.CreateInstance<ModerationInvitePurgingHandler>();

            var services = new ServiceCollection()
                .AddMediator()
                .AddSingleton<INotificationHandler<ChatMessageReceived>, ModerationInvitePurgingHandler>(p => sut)
                .BuildServiceProvider();

            var mediator = services.GetService<IMediator>();
            var msg = MockMessage("https://discord.gg/invite/asdf");
            await mediator.Publish(msg);

            autoMocker.GetMock<IModerationService>().Verify(m => m.DeleteMessageAsync(msg.Message, It.IsAny<string>()));
        }

        private ChatMessageReceived MockMessage(string content)
        {
            var msg = Mock.Of<IMessage>(ctx =>
                ctx.Content == content
                && ctx.Author == Mock.Of<IGuildUser>(p => p.Id == 10180085)
                && ctx.Channel == GetMockChannel()
            );
            return new ChatMessageReceived(msg);
        }

        private static IMessageChannel GetMockChannel() =>
            new Mock<IMessageChannel>()
                .As<ITextChannel>()
                .As<IGroupChannel>()
                .Object;
    }
}
