using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    public class UserInfoModule : ModuleBase
    {
        private const string Format = "{0}: {1} ago ({2:yyyy-MM-ddTHH:mm:ssK})\n";

        //optimization: UtcNow is slow and the module is created per-request
        private readonly DateTime _utcNow = DateTime.UtcNow;

        public UserInfoModule(ILogger<UserInfoModule> logger, IUserService userService, IModerationService moderationService, IAuthorizationService authorizationService, IMessageRepository messageRepository)
        {
            Log = logger ?? new NullLogger<UserInfoModule>();
            UserService = userService;
            ModerationService = moderationService;
            AuthorizationService = authorizationService;
            MessageRepository = messageRepository;
        }

        private ILogger<UserInfoModule> Log { get; }
        private IUserService UserService { get; }
        private IModerationService ModerationService { get; }
        private IAuthorizationService AuthorizationService { get; }
        private IMessageRepository MessageRepository { get; }

        [Command("info")]
        public async Task GetUserInfo(DiscordUserEntity user = null)
        {
            user = user ?? new DiscordUserEntity(Context.User.Id);

            var userInfo = await UserService.GetUserInformationAsync(Context.Guild.Id, user.Id);

            if (userInfo == null)
            {
                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithTitle("Retrieval Error")
                    .WithColor(Color.Red)
                    .WithDescription("Sorry, we don't have any data for that user - and we couldn't find any, either.")
                    .AddField("User Id", user.Id)
                    .Build());

                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("**\u276F User Information**");
            builder.AppendLine("ID: " + userInfo.Id);
            builder.AppendLine("Profile: " + MentionUtils.MentionUser(userInfo.Id));

            if (userInfo.IsBanned)
            {
                builder.AppendLine("Status: **Banned** \\🔨");

                if (await AuthorizationService.HasClaimsAsync(Context.User as IGuildUser, AuthorizationClaim.ModerationRead))
                {
                    builder.AppendLine($"Ban Reason: {userInfo.BanReason}");
                }
            }
            else
            {
                builder.AppendLine($"Status: {userInfo.Status.Humanize()}");
            }

            if (userInfo.FirstSeen is DateTimeOffset firstSeen)
                builder.Append(FormatTimeAgo("First Seen", firstSeen));

            if (userInfo.LastSeen is DateTimeOffset lastSeen)
                builder.Append(FormatTimeAgo("Last Seen", lastSeen));

            try
            {
                await AddParticipationToEmbed(user.Id, builder);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "An error occured while retrieving a user's message count.");
            }

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(userInfo.Username + "#" + userInfo.Discriminator)
                .WithColor(new Color(253, 95, 0))
                .WithTimestamp(_utcNow);

            embedBuilder.ThumbnailUrl = userInfo.GetAvatarUrl();
            embedBuilder.Author.IconUrl = userInfo.GetAvatarUrl();
            
            AddMemberInformationToEmbed(userInfo, builder, embedBuilder);

            if (await AuthorizationService.HasClaimsAsync(Context.User as IGuildUser, AuthorizationClaim.ModerationRead))
            {
                await AddInfractionsToEmbed(user.Id, builder);
            }

            embedBuilder.Description = builder.ToString();

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }

        private void AddMemberInformationToEmbed(EphemeralUser member, StringBuilder builder, EmbedBuilder embedBuilder)
        {
            builder.AppendLine();
            builder.AppendLine("**\u276F Member Information**");

            if (!string.IsNullOrEmpty(member.Nickname))
            {
                builder.AppendLine("Nickname: " + member.Nickname);
            }

            builder.Append(FormatTimeAgo("Created", member.CreatedAt));

            if (member.JoinedAt is DateTimeOffset joinedAt)
            {
                builder.Append(FormatTimeAgo("Joined", joinedAt));
            }

            if (member.RoleIds?.Count > 0)
            {
                var roles = member.RoleIds.Select(x => member.Guild.Roles.Single(y => y.Id == x))
                    .Where(x => x.Id != x.Guild.Id) // @everyone role always has same ID than guild
                    .ToArray();

                if (roles.Length > 0)
                {
                    Array.Sort(roles); // Sort by position: lowest positioned role is first
                    Array.Reverse(roles); // Reverse the sort: highest positioned role is first

                    builder.Append(roles.Length > 1 ? "Roles: " : "Role: ");
                    builder.AppendLine(roles.Select(r => r.Mention).Humanize());
                }
            }

            embedBuilder.Color = GetDominantColor(member);
        }

        private async Task AddInfractionsToEmbed(ulong userId, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine($"**\u276F Infractions [See here](https://mod.gg/infractions?subject={userId})**");

            var counts = await ModerationService.GetInfractionCountsForUserAsync(userId);

            builder.AppendLine(FormatUtilities.FormatInfractionCounts(counts));
        }

        private async Task AddParticipationToEmbed(ulong userId, StringBuilder builder)
        {
            var messagesByDate = await MessageRepository.GetGuildUserMessageCountByDate(Context.Guild.Id, userId, TimeSpan.FromDays(30));

            var lastWeek = _utcNow - TimeSpan.FromDays(7);

            var weekTotal = 0;
            var monthTotal = 0;
            foreach (var kvp in messagesByDate)
            {
                if (kvp.Key >= lastWeek)
                {
                    weekTotal += kvp.Value;
                }

                monthTotal += kvp.Value;
            }

            builder.AppendLine();
            builder.AppendLine("**\u276F Guild Participation**");
            builder.AppendLine("Last 7 days: " + weekTotal + " messages");
            builder.AppendLine("Last 30 days: " + monthTotal + " messages");

            if (monthTotal > 0)
            {
                try
                {
                    var channels = await MessageRepository.GetGuildUserMessageCountByChannel(Context.Guild.Id, userId, TimeSpan.FromDays(30));

                    foreach (var kvp in channels.OrderByDescending(x => x.Value))
                    {
                        var channel = await Context.Guild.GetChannelAsync(kvp.Key);

                        if (channel.IsPublic())
                        {
                            builder.AppendLine($"Most active channel: {MentionUtils.MentionChannel(channel.Id)} ({kvp.Value} messages)");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug(ex, "Unable to get the most active channel for {UserId}.", userId);
                }
            }
        }

        private string FormatTimeAgo(string prefix, DateTimeOffset ago)
        {
            var span = _utcNow - ago;

            var humanizedTimeAgo = span > TimeSpan.FromSeconds(60)
                ? span.Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture)
                : "a few seconds";

            return string.Format(CultureInfo.InvariantCulture, Format, prefix, humanizedTimeAgo, ago.UtcDateTime);
        }

        private static Color GetDominantColor(IGuildUser user)
        {
            // TODO: Get the dominate image in the user's avatar.
            return new Color(253, 95, 0);
        }
    }
}
