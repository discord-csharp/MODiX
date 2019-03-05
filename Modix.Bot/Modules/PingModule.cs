using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Modules
{
    public class PingModule : ModuleBase
    {
        public PingModule(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
        }

        [Command("ping")]
        public Task Ping() =>
            ReplyAsync($"Pong! ({DiscordClient.Latency} ms)");

        protected DiscordSocketClient DiscordClient { get; }
    }
}
