using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;
using Moq;
using NUnit.Framework;

namespace Modix.Services.Test
{
    public class MediatrIntegrationTests
    {
        [Test]
        public async Task AddMediatrBinding_Always_BindsHandlersCorrectly()
        {
            // this tests that the DI container bindings to set up mediatr
            // are working properly
            var mock = new Mock<INotificationHandler<ChatMessageReceived>>();
            var mockHandler = mock.Object;

            var services = new ServiceCollection()
                .AddMediator() // the setup this method is what's being tested
                .AddSingleton(p => mockHandler)
                .BuildServiceProvider();

            var mediator = services.GetService<IMediator>();
            var msg = MockMessage("https://discord.gg/invite/asdf");
            await mediator.Publish(msg);

            mock.ShouldHaveReceived(x => x.Handle(msg, It.IsAny<CancellationToken>()));
        }

        private ChatMessageReceived MockMessage(string content)
        {
            var msg = Mock.Of<IMessage>(ctx =>
                ctx.Content == content
                && ctx.Author == Mock.Of<IGuildUser>(p => p.Id == 9999999)
                && ctx.Channel == GetMockChannel()
            );
            return new ChatMessageReceived {Message = msg };
        }

        private static IMessageChannel GetMockChannel() =>
            new Mock<IMessageChannel>()
                .As<ITextChannel>()
                .As<IGroupChannel>()
                .Object;
    }
}
