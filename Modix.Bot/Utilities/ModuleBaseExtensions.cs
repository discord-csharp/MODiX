using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Modix.Utilities
{
    public static class ModuleBaseExtensions
    {
        public static async Task AddConfirmation(this ModuleBase moduleBase)
            => await moduleBase.Context.Message.AddReactionAsync(new Emoji("✅"));
    }
}
