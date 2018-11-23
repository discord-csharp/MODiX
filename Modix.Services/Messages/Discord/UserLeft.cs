using Discord;
using MediatR;

namespace Modix.Services.Messages.Discord
{
    public class UserLeft : INotification
    {
        public IGuild Guild { get; set; }
    }
}
