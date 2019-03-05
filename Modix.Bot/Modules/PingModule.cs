using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Modules
{
    public sealed class PingModule : ModuleBase
    {
        public PingModule(DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        [Command("ping")]
        public Task Ping() =>
            ReplyAsync($"Pong! ({_discordClient.Latency} ms)");

        private DiscordSocketClient _discordClient;
    }
}
