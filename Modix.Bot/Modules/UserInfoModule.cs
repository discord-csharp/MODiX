﻿using System;
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
using Modix.Data.Models.Core;
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

        public UserInfoModule(ILogger<UserInfoModule> logger, IUserService userService, IModerationService moderationService, IAuthorizationService authorizationService)
        {
            Log = logger ?? new NullLogger<UserInfoModule>();
            UserService = userService;
            ModerationService = moderationService;
            AuthorizationService = authorizationService;
        }

        private ILogger<UserInfoModule> Log { get; }
        private IUserService UserService { get; }
        private IModerationService ModerationService { get; }
        private IAuthorizationService AuthorizationService { get; }

        [Command("info")]
        public async Task GetUserInfo(IGuildUser user = null)
        {
            await GetUserInfoFromId(user?.Id ?? Context.User.Id);
        }

        [Command("info")]
        public async Task GetUserInfoFromId(ulong userId)
        {
            var userSummary = await UserService.GetGuildUserSummaryAsync(Context.Guild.Id, userId);

            if (userSummary == null)
            {
                await ReplyAsync("We don't have any data for that user.");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("**\u276F User Information**");
            builder.AppendLine("ID: " + userSummary.UserId);
            builder.AppendLine("Profile: " + MentionUtils.MentionUser(userSummary.UserId));

            // TODO: Add content about the user's presence, if any

            builder.Append(FormatTimeAgo("First Seen", userSummary.FirstSeen));
            builder.Append(FormatTimeAgo("Last Seen", userSummary.LastSeen));

            var embedBuilder = new EmbedBuilder()
                .WithAuthor(userSummary.Username + "#" + userSummary.Discriminator)
                .WithColor(new Color(253, 95, 0))
                .WithTimestamp(_utcNow);

            if (await UserService.GuildUserExistsAsync(Context.Guild.Id, userId))
            {
                IGuildUser member = await UserService.GetGuildUserAsync(Context.Guild.Id, userId);
                AddMemberInformationToEmbed(member, builder, embedBuilder);
            }
            else
            {
                builder.AppendLine();
                builder.AppendLine("**\u276F No Member Information**");
            }

            if (await AuthorizationService.HasClaimsAsync(Context.User as IGuildUser, AuthorizationClaim.ModerationRead))
            {
                await AddInfractionsToEmbed(userId, builder);
            }

            embedBuilder.Description = builder.ToString();

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }

        private void AddMemberInformationToEmbed(IGuildUser member, StringBuilder builder, EmbedBuilder embedBuilder)
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

            if (member.RoleIds.Count > 0)
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
            embedBuilder.ThumbnailUrl = member.GetAvatarUrl();
            embedBuilder.Author.IconUrl = member.GetAvatarUrl();
        }

        private async Task AddInfractionsToEmbed(ulong userId, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine($"**\u276F Infractions [See here](https://mod.gg/infractions?subject={userId})**");

            var counts = await ModerationService.GetInfractionCountsForUserAsync(userId);

            builder.AppendLine(FormatUtilities.FormatInfractionCounts(counts));
        }

        private string FormatTimeAgo(string prefix, DateTimeOffset ago)
        {
            var humanizedTimeAgo = (_utcNow - ago).Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture);
            return string.Format(CultureInfo.InvariantCulture, Format, prefix, humanizedTimeAgo, ago.UtcDateTime);
        }

        private static Color GetDominantColor(IUser user)
        {
            // TODO: Get the dominate image in the user's avatar.
            return new Color(253, 95, 0);
        }
    }
}
