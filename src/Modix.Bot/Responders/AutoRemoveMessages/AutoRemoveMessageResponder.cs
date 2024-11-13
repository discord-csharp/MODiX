using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Bot.Notifications;

namespace Modix.Bot.Responders.AutoRemoveMessages;

public class AutoRemoveMessageResponder(AutoRemoveMessageService service) :
    INotificationHandler<ReactionAddedNotificationV3>
{
    public async Task Handle(ReactionAddedNotificationV3 notification, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested
            || notification.Reaction.Emote.Name != "❌"
            || !service.IsKnownRemovableMessage(notification.Message.Id, out var cachedMessage)
            || !cachedMessage.Users.Any(user => user.Id == notification.Reaction.UserId))
        {
            return;
        }

        await cachedMessage.Message.DeleteAsync();

        service.UnregisterRemovableMessage(cachedMessage.Message);
    }
}
