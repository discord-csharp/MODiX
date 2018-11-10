using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Modules
{
    public class PingModule : ModuleBase
    {
        [Command("ping")]
        public Task Ping() =>
            ReplyAsync("Pong!");
    }
}
