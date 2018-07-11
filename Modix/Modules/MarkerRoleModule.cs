using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix.Modules
{
    [Group("role"), Name("Marker Role Manager"), Summary("Allows you to add and remove specific marker roles.")]
    public class MarkerRoleModule : ModuleBase
    {
        [Command("add")]
        public async Task AddRole([Remainder]IRole targetRole)
        {
            var guildUser = (IGuildUser)Context.User;
            await guildUser.AddRoleAsync(targetRole);
        }
        
        [Command("remove")]
        public async Task RemoveRole([Remainder]IRole targetRole)
        {
            var guildUser = (IGuildUser)Context.User;
            await guildUser.RemoveRoleAsync(targetRole);
        }
    }
}