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
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("User Info"), Summary("Various ways to get info about a user")]
    public class UserInfoModule : ModuleBase
    {
        private IModerationService _moderationService;

        public UserInfoModule(ILogger<UserInfoModule> logger, IModerationService moderationService)
        {
            Log = logger ?? new NullLogger<UserInfoModule>();
            _moderationService = moderationService;
        }

        private ILogger<UserInfoModule> Log { get; }

        [Command("info"), Summary("Retrieve a user's basic information.")]
        public async Task GetUserInfo(IUser user = null)
        {
            user = user ?? Context.User;

            var utcNow = DateTime.UtcNow;
            var builder = new StringBuilder();
            builder.AppendLine("**\u276F User Information**");
            builder.AppendLine("ID: " + user.Id);
            builder.AppendLine("Profile: " + user.Mention);

            // TODO: Add content about the user's presence, if any

            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "Created: {0} ago ({1:yyyy-MM-ddTHH:mm:ssK})\n",
                (utcNow - user.CreatedAt).Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture),
                user.CreatedAt.UtcDateTime);

            if (user is IGuildUser member)
            {
                builder.AppendLine();
                builder.AppendLine("**\u276F Member Information**");

                if (!string.IsNullOrEmpty(member.Nickname))
                {
                    builder.AppendLine("Nickname: " + member.Nickname);
                }

                if (member.JoinedAt is DateTimeOffset joinedAt)
                {
                    builder.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "Joined: {0} ago ({1:yyyy-MM-ddTHH:mm:ssK})\n",
                        (utcNow - joinedAt).Humanize(maxUnit: TimeUnit.Year, culture: CultureInfo.InvariantCulture),
                        joinedAt.UtcDateTime);
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
                        builder.AppendLine(BuildList(roles, r => r.Mention));
                    }
                }
            }

            //Filter out notes
            var infractions = await _moderationService.SearchInfractionsAsync(new InfractionSearchCriteria
            {
                SubjectId = user.Id,
                IsRescinded = false,
                IsDeleted = false,
                Types = new InfractionType[] { InfractionType.Ban, InfractionType.Mute, InfractionType.Warning }
            });

            builder.AppendLine($"Active Infractions: {infractions.Count}");
            
            // TODO: Add voice session data

            var embedBuilder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = user.Username + "#" + user.Discriminator,
                    IconUrl = user.GetAvatarUrl()
                },
                Color = GetDominateColor(user),
                Description = builder.ToString(),
                ThumbnailUrl = user.GetAvatarUrl()
            };

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }

        [Command("info")]
        public async Task GetUserInfo(ulong userId)
        {
            if (userId == Context.User.Id)
            {
                await GetUserInfo(Context.User);
                return;
            }

            var user = await Context.Guild.GetUserAsync(userId) ?? await Context.Client.GetUserAsync(userId);
            if (!(user is null))
            {
                await GetUserInfo(user);
                return;
            }

            // TODO: Check the database to see if we have anything about the user in our database.
            await ReplyAsync("User not found.");
        }

        private static string BuildList<T>(T[] array, Func<T, string> mapper)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                builder.Append(GetListSeparator(i, array.Length) + mapper(array[i]));
            }

            return builder.ToString();
        }

        private static string GetListSeparator(int i, int length)
        {
            bool atLastIndex = i == length - 1;
            if (i == 0)
            {
                return string.Empty;
            }
            else if (length > 2 && atLastIndex)
            {
                return ", and ";
            }
            else if (length > 1 && atLastIndex)
            {
                return " and ";
            }
            else
            {
                return ", ";
            }
        }

        private static Color GetDominateColor(IUser user)
        {
            // TODO: Get the dominate image in the user's avatar.
            return new Color(253, 95, 0);
        }
    }
}
