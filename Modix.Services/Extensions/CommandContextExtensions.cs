#nullable enable
using System.Threading.Tasks;
using Discord;

namespace Modix.Services.Extensions
{
    public static class CommandContextExtensions
    {
        public static Task<IUserMessage> ReplyAsync(this IMessage context,
            string message, Embed? embed = null)
        {
            return context.Channel.SendMessageAsync(message, embed: embed);
        }
    }
}
