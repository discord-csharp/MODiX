using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Humanizer;

using Modix.Data.Models;
using Modix.Data.Repositories;
using Modix.Services.CommandHelp;
using Modix.Services.Images;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [ModuleHelp("Guild Information", "Retrieves information and statistics about the supplied guild.")]
    [HelpTags("guildinfo")]
    public sealed class GuildInfoModule : InteractionModuleBase
    {
        //optimization: UtcNow is slow and the module is created per-request
        private readonly DateTime _utcNow = DateTime.UtcNow;

        public GuildInfoModule(
            DiscordSocketClient discordClient,
            IMessageRepository messageRepository,
            IEmojiRepository emojiRepository,
            IImageService imageService)
        {
            _discordClient = discordClient;
            _messageRepository = messageRepository;
            _emojiRepository = emojiRepository;
            _imageService = imageService;
        }

        private readonly DiscordSocketClient _discordClient;
        private readonly IMessageRepository _messageRepository;
        private readonly IEmojiRepository _emojiRepository;
        private readonly IImageService _imageService;

        [SlashCommand("guildinfo", "Retrieves information about the supplied guild, or the current guild if one is not provided.")]
        public async Task GetGuildInfoAsync()
        {
            var timer = Stopwatch.StartNew();

            var guildId = Context.Guild.Id;
            var guild = _discordClient.GetGuild(guildId);

            if (guild is null)
            {
                await FollowupAsync(embed: new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, I have no information about that guild")
                    .AddField("Guild Id", guildId)
                    .Build());
            }
            else
            {
                var embedBuilder = new EmbedBuilder()
                    .WithAuthor(guild.Name, guild.IconUrl)
                    .WithThumbnailUrl(guild.IconUrl)
                    .WithTimestamp(_utcNow);

                await WithDominantColorAsync(embedBuilder, guild);

                var stringBuilder = new StringBuilder();

                AppendGuildInformation(stringBuilder, guild);

                await AppendGuildParticipationAsync(stringBuilder, guild);
                AppendMemberInformation(stringBuilder, guild);

                AppendRoleInformation(stringBuilder, guild);

                embedBuilder.WithDescription(stringBuilder.ToString());

                timer.Stop();
                embedBuilder.WithFooter($"Completed after {timer.ElapsedMilliseconds} ms");

                await FollowupAsync(embed: embedBuilder.Build());
            }
        }

        public async Task WithDominantColorAsync(EmbedBuilder embedBuilder, IGuild guild)
        {
            if (guild.IconUrl is { } iconUrl)
            {
                var color = await _imageService.GetDominantColorAsync(new Uri(iconUrl));
                embedBuilder.WithColor(color);
            }
        }

        public void AppendGuildInformation(StringBuilder stringBuilder, SocketGuild guild)
        {
            stringBuilder
                .AppendLine(Format.Bold("\u276F Guild Information"))
                .AppendLine($"ID: {guild.Id}")
                .AppendLine($"Owner: {MentionUtils.MentionUser(guild.OwnerId)}")
                .AppendLine($"Created: {FormatUtilities.FormatTimeAgo(_utcNow, guild.CreatedAt)}")
                .AppendLine();
        }

        public async Task AppendGuildParticipationAsync(StringBuilder stringBuilder, SocketGuild guild)
        {
            var weekTotal = await _messageRepository.GetTotalMessageCountAsync(guild.Id, TimeSpan.FromDays(7));
            var monthTotal = await _messageRepository.GetTotalMessageCountAsync(guild.Id, TimeSpan.FromDays(30));

            var channelCounts = await _messageRepository.GetTotalMessageCountByChannelAsync(guild.Id, TimeSpan.FromDays(30));
            var orderedChannelCounts = channelCounts.OrderByDescending(x => x.Value);
            var mostActiveChannel = orderedChannelCounts.FirstOrDefault();

            stringBuilder
                .AppendLine(Format.Bold("\u276F Guild Participation"))
                .AppendLine($"Last 7 days: {"message".ToQuantity(weekTotal, "n0")}")
                .AppendLine($"Last 30 days: {"message".ToQuantity(monthTotal, "n0")}")
                .AppendLine($"Avg. per day: {"message".ToQuantity(monthTotal / 30, "n0")}");

            stringBuilder
                .AppendLine($"Most active channel: {MentionUtils.MentionChannel(mostActiveChannel.Key)} ({"message".ToQuantity(mostActiveChannel.Value, "n0")} in 30 days)");

            var emojiCounts = await _emojiRepository.GetEmojiStatsAsync(guild.Id, SortDirection.Ascending, 1);

            if (emojiCounts.Any())
            {
                var favoriteEmoji = emojiCounts.First();

                var emojiFormatted = ((SocketSelfUser)Context.Client.CurrentUser).CanAccessEmoji(favoriteEmoji.Emoji)
                    ? favoriteEmoji.Emoji.ToString()
                    : $"{Format.Url("❔", favoriteEmoji.Emoji.Url)} (`{favoriteEmoji.Emoji.Name}`)";

                stringBuilder.AppendLine($"Favorite emoji: {emojiFormatted} ({"time".ToQuantity(favoriteEmoji.Uses)})");
            }

            stringBuilder.AppendLine();
        }

        public static void AppendMemberInformation(StringBuilder stringBuilder, SocketGuild guild)
        {
            var members = guild.Users.Count;
            var bots = guild.Users.Count(x => x.IsBot);
            var humans = members - bots;

            stringBuilder
                .AppendLine(Format.Bold("\u276F Member Information"))
                .AppendLine($"Total member count: {members}")
                .AppendLine($"• Humans: {humans}")
                .AppendLine($"• Bots: {bots}")
                .AppendLine();
        }

        public static void AppendRoleInformation(StringBuilder stringBuilder, SocketGuild guild)
        {
            var roles = guild.Roles
                .Where(x => x.Id != guild.EveryoneRole.Id && x.Color != Color.Default)
                .OrderByDescending(x => x.Position)
                .ThenByDescending(x => x.IsHoisted);

            stringBuilder.AppendLine(Format.Bold("\u276F Guild Roles"))
                .AppendLine(roles.Select(x => x.Mention).Humanize())
                .AppendLine();
        }
    }
}
