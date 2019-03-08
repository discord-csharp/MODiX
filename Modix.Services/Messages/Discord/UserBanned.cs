using Discord;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class UserBanned : INotification
    {
        public IUser BannedUser { get; set; }

        public IGuild Guild { get; set; }
    }
}
