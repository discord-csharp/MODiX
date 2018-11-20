using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix
{
    public static class CommandContextExtensions
    {
        private static readonly Emoji checkmarkEmoji = new Emoji("✅");

        public static Task AddConfirmation(this ICommandContext context) =>
            context.Message.AddReactionAsync(checkmarkEmoji);
    }
}
