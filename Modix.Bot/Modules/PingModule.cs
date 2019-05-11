using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Modix.Data;

namespace Modix.Modules
{
    [Name("Ping")]
    [Summary("Provides commands related to determining connectivity and latency.")]
    public sealed class PingModule : ModuleBase
    {
        private Dictionary<string, Func<Task<double>>> _latencyProviders;

        public PingModule(DiscordSocketClient discordClient, DiscordRestClient restClient)
        {
            _discordClient = discordClient;
            _restClient = restClient;

            _latencyProviders = new Dictionary<string, Func<Task<double>>>
            {
                ["Discord Gateway"] = () => Task.FromResult((double)_discordClient.Latency),
                ["Discord REST"] = async () =>
                {
                    var watch = Stopwatch.StartNew();
                    await restClient.GetRecommendedShardCountAsync();
                    watch.Stop();

                    return watch.ElapsedMilliseconds;
                },
                ["Google"] = async () => (await new Ping().SendPingAsync("8.8.8.8")).RoundtripTime,
                ["Cloudflare"] = async () => (await new Ping().SendPingAsync("1.1.1.1")).RoundtripTime
            };
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency.")]
        public async Task Ping()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Pong!");

            var results = await Task.WhenAll
            (
                _latencyProviders
                    .Select(async d => (name: d.Key, latency: await d.Value()))
            );

            var average = results.Average(d => d.latency);

            embed = embed
                .WithCurrentTimestamp()
                .WithFooter($"Average: {FormatLatency(average)}")
                .WithColor(LatencyColor(average));

            foreach (var (name, latency) in results)
            {
                embed.AddField(name, FormatLatency(latency), true);
            }

            await ReplyAsync(embed: embed.Build());
        }

        private string FormatLatency(double latency)
        {
            var suffix = latency switch
            {
                _ when latency > 250 => "💔",
                _ when latency > 100 => "💛", //Yellow heart - trust me
                _ when latency < 100 && latency >= 0 => "💚", //Green heart - trust me again
                _ => "❓"
            };

            return $"📶 {latency: 0}ms {suffix}";
        }

        private Color LatencyColor(double avg)
            => avg switch
            {
                _ when avg > 250 => Color.Red,
                _ when avg > 100 => Color.Gold,
                _ when avg < 100 && avg >= 0 => Color.Green,
                _ => Color.Default
            };

        private DiscordSocketClient _discordClient;
        private readonly DiscordRestClient _restClient;
    }
}
