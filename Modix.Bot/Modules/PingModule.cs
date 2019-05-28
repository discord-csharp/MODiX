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
using Discord.Rest;
using Discord.WebSocket;

namespace Modix.Modules
{
    [Name("Ping")]
    [Summary("Provides commands related to determining connectivity and latency.")]
    public sealed class PingModule : ModuleBase
    {
        private Dictionary<string, Func<Task<double>>> _latencyProviders;
        private Dictionary<string, Func<Task<bool>>> _availabilityProviders;

        private const int PingTimeout = 2000;

        public PingModule(DiscordSocketClient discordClient, DiscordRestClient restClient, IHttpClientFactory httpClientFactory)
        {
            _discordClient = discordClient;
            _restClient = restClient;
            _httpClient = httpClientFactory.CreateClient();

            //Some APIs require this
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "github.com/discord-csharp/MODiX");

            _latencyProviders = new Dictionary<string, Func<Task<double>>>
            {
                ["Discord Gateway"] = () => Task.FromResult((double)_discordClient.Latency),
                ["Google"] = async () => (await new Ping().SendPingAsync("8.8.8.8", PingTimeout)).RoundtripTime,
                ["Cloudflare"] = async () => (await new Ping().SendPingAsync("1.1.1.1", PingTimeout)).RoundtripTime
            };

            _availabilityProviders = new Dictionary<string, Func<Task<bool>>>
            {
                ["Github REST"] = () => EndpointAvailable("https://api.github.com"),
                ["Discord REST"] = () => EndpointAvailable(() => _restClient.GetApplicationInfoAsync())
            };
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency.")]
        public async Task Ping()
        {
            var message = await ReplyAsync($"Pinging {_latencyProviders.Count} latency endpoints " +
                $"and {_availabilityProviders.Count} availability endpoints...");

            var embed = new EmbedBuilder()
                .WithTitle($"Pong! Checked {_latencyProviders.Count + _availabilityProviders.Count} endpoints");

            var latencyResults = await Task.WhenAll
            (
                _latencyProviders
                    .Select(async d =>
                    {
                        try
                        {
                            var latency = await d.Value();
                            return (name: d.Key, latency);
                        }
                        catch (PingException)
                        {
                            return (name: d.Key, latency: -1);
                        }
                    })
            );

            var availabilityResults = await Task.WhenAll
            (
                _availabilityProviders
                    .Select(async d => (name: d.Key, isUp: await d.Value()))
            );

            var average = latencyResults
                .Select(d => d.latency)
                .Where(d => d > -1)
                .Average();

            embed = embed
                .WithCurrentTimestamp()
                .WithColor(LatencyColor(average));

            foreach (var (name, latency) in latencyResults)
            {
                embed.AddField(name, FormatLatency(latency), true);
            }

            foreach (var (name, isUp) in availabilityResults)
            {
                embed.AddField(name, FormatAvailability(isUp), true);
            }

            embed.AddField("Average", FormatLatency(average, "📈"), true);

            await message.ModifyAsync(m =>
            {
                m.Embed = embed.Build();
                m.Content = "";
            });
        }

        private string FormatLatency(double latency, string icon = "📶")
        {
            if (latency == -1)
            {
                return $"{icon} Error ⚠️";
            }

            var suffix = latency switch
            {
                _ when latency > 300 => "💔",
                _ when latency > 100 => "💛", //Yellow heart - trust me
                _ when latency < 100 => "💚", //Green heart - trust me again
                _ => "❓"
            };

            return $"{icon} {latency: 0}ms {suffix}";
        }

        private string FormatAvailability(bool isUp)
        {
            return $"🌐 {(isUp ? "Up" : "Down")} {(isUp ? "💚" : "💔")}";
        }

        private Color LatencyColor(double avg)
            => avg switch
            {
                _ when avg > 300 || avg < 0 => Color.Red,
                _ when avg > 100 => Color.Gold,
                _ when avg < 100 => Color.Green,
                _ => Color.Default
            };


        private async Task<bool> EndpointAvailable(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, new CancellationTokenSource(PingTimeout).Token);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private async Task<bool> EndpointAvailable(Func<Task> endpoint)
        {
            try
            {
                await endpoint();
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private readonly DiscordSocketClient _discordClient;
        private readonly HttpClient _httpClient;
        private readonly DiscordRestClient _restClient;
    }
}
