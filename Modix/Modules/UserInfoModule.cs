namespace Modix.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Humanizer;
    using Humanizer.Localisation;

    [Summary("Used to show information about a user.")]
    public class UserInfoModule : ModuleBase
    {

        [Command("info"), Summary("Shows Information for a specific user")]
        public async Task Info(SocketUser user = null)
        {
            var userInfo = user ?? Context.User;
            var uname = userInfo.Username ?? "?";

            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = userInfo.IsBot ? $"{uname} (Bot)" : $"{uname}",
                    IconUrl = userInfo.GetAvatarUrl()
                },
                Color = new Color(24, 138, 184),
                ThumbnailUrl = userInfo.GetAvatarUrl()
            };
            var socketUser = userInfo as SocketGuildUser;
            builder.WithTitle("User Information");
;           builder.AddInlineField("ID: ", userInfo.Id);
            if (socketUser.Nickname != null) builder.AddInlineField("Nickname: ", socketUser.Nickname);
            builder.AddInlineField("Profile: ", userInfo.Mention);
            builder.AddInlineField("Created: ", $"{(DateTime.Now - userInfo.CreatedAt).Humanize(2, null, TimeUnit.Year)} ago ({userInfo.CreatedAt})");
            builder.AddInlineField("Current Status: ", userInfo.Status);
            builder.AddInlineField("Joined: ", $"{(DateTimeOffset.Now - socketUser.JoinedAt.Value).Humanize(2, null, TimeUnit.Year)} ago ({socketUser.JoinedAt})");

            var channels = await Context.Guild.GetTextChannelsAsync();

            var messages = new List<IMessage>();
            foreach (var channel in channels)
            {
                messages.AddRange(channel.GetMessagesAsync().Flatten().Result.ToList());
            }


            var firstMessage = messages.Where(m => m.Author.Id == userInfo.Id).OrderBy(x => x.CreatedAt).First();
            builder.AddInlineField("First Message: ", $"{(DateTime.Now - firstMessage.CreatedAt).Humanize(2, null, TimeUnit.Year)} ago ({firstMessage.CreatedAt})");

            var lastMessage = messages.Where(m => m.Author.Id == userInfo.Id).OrderByDescending(x => x.CreatedAt).First();
            builder.AddInlineField("First Message: ", $"{(DateTime.Now - lastMessage.CreatedAt).Humanize(2, null, TimeUnit.Year)} ago ({lastMessage.CreatedAt})");

            await ReplyAsync("", embed: builder.Build());
        }
    }
}