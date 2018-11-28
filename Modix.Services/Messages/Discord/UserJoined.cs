using Discord;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class UserJoined : INotification
    {
        public IGuild Guild { get; set; }
    }
}
