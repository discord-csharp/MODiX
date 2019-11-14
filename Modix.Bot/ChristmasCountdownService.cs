using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.DateTime;

namespace Modix
{
    public sealed class ChristmasCountdownService : BackgroundService
    {
        private readonly DiscordSocketClient discord;

        public ChristmasCountdownService(DiscordSocketClient discordClient, ILogger<ChristmasCountdownService> logger)
        {
            discord = discordClient;
            Log = logger;
        }

        private ILogger<ChristmasCountdownService> Log { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.LogInformation("Starting Christmas Countdown background service.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = UtcNow;
                var christmas = new DateTime(now.Year, 12, 25, 0, 0, 0, DateTimeKind.Utc);
                var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

                if (now is { Month: 12, Day: 25 })
                {
                    Log.LogInformation("Today is Christmas Day!!!");

                    var cheer = GenerateGoodTidingsOfComfortAndJoy();

                    await SetChannelName(cheer).ConfigureAwait(false);
                }
                else
                {
                    Log.LogDebug("Christmas is coming!");

                    var daysRemaining = christmas.Subtract(now).TotalDays;

                    var name = today switch
                    {
                        { DayOfWeek: DayOfWeek.Tuesday } when today.Hour < 18 => $"🎅 {daysRemaining} Days (Caroling Practice TODAY at 6:00 PM)",
                        _ => $"🎅 {daysRemaining} Days"
                    };

                    await SetChannelName(name).ConfigureAwait(false);
                }

                async Task SetChannelName(string channelName) =>
                    await discord.GetGuild(143867839282020352)
                        .GetChannel(610469022852579358)
                        .ModifyAsync(channel => channel.Name = channelName)
                        .ConfigureAwait(false);

                static string GenerateGoodTidingsOfComfortAndJoy()
                {
                    var comfortAndJoy = new[]
                    {
                        "Merry Christmas! 🎁 🔔🎄",
                        "🎶🔔 Jingle Bells 🔔🎶",
                        "Ho Ho Ho!!! 🎅",
                        "🎅🎉 🎁 🔔🎄",
                        "🥛 🍪 🎅",
                        "🦌 Rudolph the Red Nosed 🔴 Reindeer!",
                        "Do You Want to Build a Snowman? 👐 ☃️ 🥕",
                        "🎄 🎁 🧦"
                        // TODO: Need more holiday cheer
                    };

                    var christmasMiracle = RandomNumberGenerator.GetInt32(comfortAndJoy.Length + 1);
                    return comfortAndJoy[christmasMiracle];
                }
            }

            Log.LogInformation("Christmas Countdown background service is stopped.");
        }
    }
}
