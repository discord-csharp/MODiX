using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Modules
{
    [Name("Ping")]
    [Summary("Provides commands related to determining connectivity and latency.")]
    public sealed class PingModule : ModuleBase
    {
        private Dictionary<string, Func<Task<double>>> _latencyProviders;
        private const int PingTimeout = 2000;

        public PingModule(DiscordSocketClient discordClient, IHttpClientFactory httpClientFactory)
        {
            _discordClient = discordClient;
            _httpClient = httpClientFactory.CreateClient();

            //Some APIs require this
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MODiX");

            _latencyProviders = new Dictionary<string, Func<Task<double>>>
            {
                ["Discord Gateway"] = () => Task.FromResult((double)_discordClient.Latency),
                ["Google"] = async () => (await new Ping().SendPingAsync("8.8.8.8", PingTimeout)).RoundtripTime,
                ["Cloudflare"] = async () => (await new Ping().SendPingAsync("1.1.1.1", PingTimeout)).RoundtripTime,
                ["Github API 🌐"] = () => GetEndpointLatency("https://api.github.com"),
                ["Discord REST 🌐"] = () => GetEndpointLatency("https://discordapp.com/api")
            };
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency.")]
        public async Task Ping()
        {
            var message = await ReplyAsync($"Pinging {_latencyProviders.Count} endpoints...");

            var embed = new EmbedBuilder()
                .WithTitle($"Pong! Pinged {_latencyProviders.Count} services");

            var results = await Task.WhenAll
            (
                _latencyProviders
                    .Select(async d =>
                    {
                        try
                        {
                            var latency = await d.Value();
                            return (name: d.Key, latency);
                        }
                        catch (Exception) //Yes, I know
                        {
                            return (name: d.Key, latency: 0);
                        }
                    })
            );

            var average = results
                .Select(d => d.latency)
                .Where(d => d > 0)
                .Average();

            embed = embed
                .WithCurrentTimestamp()
                .WithFooter($"Average: {average: 0}ms")
                .WithColor(LatencyColor(average));

            foreach (var (name, latency) in results)
            {
                embed.AddField(name, FormatLatency(latency), true);
            }

            await message.ModifyAsync(m =>
            {
                m.Embed = embed.Build();
                m.Content = "";
            });
        }

        private string FormatLatency(double latency)
        {
            if (latency == 0)
            {
                return "📶 Error ⚠️";
            }

            var suffix = latency switch
            {
                _ when latency > 250 => "💔",
                _ when latency > 100 => "💛", //Yellow heart - trust me
                _ when latency < 100 => "💚", //Green heart - trust me again
                _ => "❓"
            };

            return $"📶 {latency: 0}ms {suffix}";
        }

        private Color LatencyColor(double avg)
            => avg switch
            {
                _ when avg > 250 || avg == 0 => Color.Red,
                _ when avg > 100 => Color.Gold,
                _ when avg < 100 => Color.Green,
                _ => Color.Default
            };


        private async Task<double> GetEndpointLatency(string url)
        {
            //We can't really ping a REST api so... just make a request and time it

            var watch = Stopwatch.StartNew();
            await _httpClient.GetAsync(url, new CancellationTokenSource(PingTimeout).Token);
            watch.Stop();

            return watch.ElapsedMilliseconds;
        }

        private DiscordSocketClient _discordClient;
        private readonly HttpClient _httpClient;
    }
}
