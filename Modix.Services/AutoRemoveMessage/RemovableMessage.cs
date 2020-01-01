using Discord;

namespace Modix.Services.AutoRemoveMessage
{
    public class RemovableMessage
    {
        public IMessage Message { get; set; }

        public IUser[] Users { get; set; }
    }
}
