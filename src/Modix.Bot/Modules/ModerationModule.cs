using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.Options;

using Modix.Bot.Extensions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Moderation")]
    [Summary("Guild moderation commands.")]
    [HelpTags("note", "warn", "mute", "tempmute", "unmute", "ban", "forceban", "unban", "clean")]
    public class ModerationModule : ModuleBase
    {
        public ModerationModule(
            ModerationService moderationService,
            IUserService userService,
            IOptions<ModixConfig> config)
        {
            ModerationService = moderationService;
            UserService = userService;
            Config = config.Value;
        }

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public async Task NoteAsync(
            [Summary("The user to which the note is being applied.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Notice, subject.UserId, reasonWithUrls, null);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public async Task WarnAsync(
            [Summary("The user to which the warning is being issued.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Warning, subject.UserId, reasonWithUrls, null);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("mute")]
        [Summary("Mute a user.")]
        public async Task MuteAsync(
            [Summary("The user to be muted.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Mute, subject.UserId, reasonWithUrls, null);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("tempmute")]
        [Alias("mute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public async Task TempMuteAsync(
            [Summary("The user to be muted.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The duration of the mute.")]
                TimeSpan duration,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Mute, subject.UserId, reasonWithUrls, duration);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("tempmute")]
        [Alias("mute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public async Task TempMuteAsync(
            [Summary("The duration of the mute.")]
                TimeSpan duration,
            [Summary("The user to be muted.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Mute, subject.UserId, reasonWithUrls, duration);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public async Task UnMuteAsync(
            [Summary("The user to be un-muted.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the unmute (optional).")]
            [Remainder]
                string reason = null)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            await ModerationService.RescindInfractionAsync(InfractionType.Mute, Context.Guild.Id, subject.UserId, reason);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("ban")]
        [Alias("forceban")]
        [Summary("Ban a user from the current guild.")]
        public async Task BanAsync(
            [Summary("The user to be banned.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.CreateInfractionAsync(Context.Guild.Id, Context.User.Id, InfractionType.Ban, subject.UserId, reasonWithUrls, null);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public async Task UnBanAsync(
            [Summary("The user to be un-banned.")]
                DiscordUserOrMessageAuthorEntity subject,
            [Summary("The reason for the unban (optional).")]
            [Remainder]
                string reason = null)
        {
            if (!await GetConfirmationIfRequiredAsync(subject))
            {
                return;
            }

            var reasonWithUrls = AppendUrlsFromMessage(reason);

            await ModerationService.RescindInfractionAsync(InfractionType.Ban, Context.Guild.Id, subject.UserId, reasonWithUrls);
            await ConfirmAndReplyWithCountsAsync(subject.UserId);
        }

        [Command("clean")]
        [Alias("prune")]
        [Summary("Mass-deletes a specified number of messages.")]
        public async Task CleanAsync(
            [Summary("The number of messages to delete.")]
                int count)
        {
            var channel = Context.Channel as ITextChannel;
            await ModerationService.DeleteMessagesAsync(
                channel, count, true,
                    () => Context.GetUserConfirmationAsync(
                        $"You are attempting to delete the past {count} messages in {channel.Mention}.{Environment.NewLine}"));
        }

        [Command("clean")]
        [Alias("prune")]
        [Summary("Mass-deletes a specified number of messages.")]
        public async Task CleanAsync(
            [Summary("The number of messages to delete.")]
                int count,
            [Summary("The channel to clean.")]
                ITextChannel channel)
            => await ModerationService.DeleteMessagesAsync(
                channel, count, Context.Channel.Id == channel.Id,
                    () => Context.GetUserConfirmationAsync(
                        $"You are attempting to delete the past {count} messages in {channel.Mention}.{Environment.NewLine}"));

        [Command("clean")]
        [Alias("prune")]
        [Summary("Mass-deletes a specified number of messages by the supplied user.")]
        public async Task CleanAsync(
            [Summary("The number of messages to delete.")]
                int count,
            [Summary("The user whose messages should be deleted.")]
                IGuildUser user)
        {
            var channel = Context.Channel as ITextChannel;
            await ModerationService.DeleteMessagesAsync(
                channel, user, count,
                    () => Context.GetUserConfirmationAsync(
                        $"You are attempting to delete the past {count} messages by {user.GetDisplayName()} in {channel.Mention}.{Environment.NewLine}"));
        }

        private async Task ConfirmAndReplyWithCountsAsync(ulong userId)
        {
            await Context.AddConfirmationAsync();

            // If the channel is public, do not list the infraction embed that occurs after a user has reached 3 infractions
            if ((Context.Channel as IGuildChannel)?.IsPublic() is true)
            {
                return;
            }

            var counts = await ModerationService.GetInfractionCountsForUserAsync(userId);

            //TODO: Make this configurable
            if (counts.Values.Any(count => count >= 3))
            {
                // https://modix.gg/infractions?subject=12345
                var url = new UriBuilder(Config.WebsiteBaseUrl)
                {
                    Path = "/infractions",
                    Query = $"subject={userId}"
                }.RemoveDefaultPort().ToString();

                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("Infraction Count Notice")
                    .WithColor(Color.Orange)
                    .WithDescription(FormatUtilities.FormatInfractionCounts(counts))
                    .WithUrl(url)
                    .Build());
            }
        }

        private string AppendUrlsFromMessage(string reason)
        {
            var urls = Context.Message.Attachments
                .Select(x => x.Url)
                .Where(x => !string.IsNullOrWhiteSpace(x));

            if (!urls.Any())
                return reason;

            return new StringBuilder(reason)
                .AppendLine()
                .AppendLine()
                .AppendJoin(Environment.NewLine, urls)
                .ToString();
        }

        private async ValueTask<bool> GetConfirmationIfRequiredAsync(DiscordUserOrMessageAuthorEntity userOrAuthor)
        {
            if (userOrAuthor.MessageId is null)
            {
                return true;
            }

            var author = await UserService.GetUserAsync(userOrAuthor.UserId);

            Debug.Assert(author is not null); // author should be nonnull, because we have a message written by someone with that ID

            return await Context.GetUserConfirmationAsync(
                "Detected a message ID instead of a user ID. Do you want to perform this action on "
                + $"{Format.Bold(author.GetDisplayName())} ({userOrAuthor.UserId}), the message's author?");
        }

        internal protected ModerationService ModerationService { get; }

        internal protected IUserService UserService { get; }

        internal protected ModixConfig Config { get; }
    }
}
