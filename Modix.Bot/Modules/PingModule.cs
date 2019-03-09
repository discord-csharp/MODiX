using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Modules
{
    [Name("Ping")]
    [Summary("Provides commands related to determining connectivity and latency.")]
    public sealed class PingModule : ModuleBase
    {
        public PingModule(DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency.")]
        public Task Ping() =>
            ReplyAsync($"Pong! ({_discordClient.Latency} ms)");

        private DiscordSocketClient _discordClient;
    }
}
