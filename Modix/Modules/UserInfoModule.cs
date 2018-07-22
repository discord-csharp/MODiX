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

namespace Modix.Modules
{
    public class UserInfoModule : ModuleBase
    {
        public UserInfoModule(ILogger<UserInfoModule> logger)
        {
            Log = logger ?? new NullLogger<UserInfoModule>();
        }

        private ILogger<UserInfoModule> Log { get; }

        [Command("info")]
        public async Task GetUserInfo([Remainder] IUser user = null)
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
                        .Where(x => !string.Equals("@everyone", x.Name, StringComparison.Ordinal))
                        .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                        .ToArray();

                    if (roles.Length > 0)
                    {
                        Array.Sort(roles, StringComparer.OrdinalIgnoreCase);
                        builder.AppendLine("Roles: " + string.Join(',', (object[])roles));
                    }
                }
            }

            // TODO: Add infraction summary

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
        public async Task GetUserInfo(ulong userId, [Remainder] string _)
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

        private static Color GetDominateColor(IUser user)
        {
            // TODO: Get the dominate image in the user's avatar.
            return new Color(253, 95, 0);
        }
    }
}
