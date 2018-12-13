using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("Moderation")]
    [Summary("Guild moderation commands")]
    public class ModerationModule : ModuleBase
    {
        public ModerationModule(IModerationService moderationService)
        {
            ModerationService = moderationService;
        }

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public async Task Note(
            [Summary("The user to which the note is being applied.")]
                DiscordUserEntity subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Notice, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public async Task Warn(
            [Summary("The user to which the warning is being issued.")]
                DiscordUserEntity subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Warning, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("mute")]
        [Summary("Mute a user.")]
        public async Task Mute(
            [Summary("The user to be muted.")]
                DiscordUserEntity subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("tempmute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public async Task TempMute(
            [Summary("The user to be muted.")]
                DiscordUserEntity subject,
            [Summary("The duration of the mute.")]
                TimeSpan duration,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, duration);
            await Context.AddConfirmation();
        }

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public async Task UnMute(
            [Summary("The user to be un-muted.")]
                DiscordUserEntity subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.Id);
        }

        [Command("ban")]
        [Alias("forceban")]
        [Summary("Ban a user from the current guild.")]
        public async Task Ban(
            [Summary("The user to be banned.")]
                DiscordUserEntity subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
        {
            await ModerationService.CreateInfractionAsync(InfractionType.Ban, subject.Id, reason, null);
            await Context.AddConfirmation();
        }

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public async Task UnBan(
            [Summary("The user to be un-banned.")]
                DiscordUserEntity subject)
        {
            await ModerationService.RescindInfractionAsync(InfractionType.Ban, subject.Id);
            await Context.AddConfirmation();
        }

        [Command("clean")]
        [Summary("Mass-deletes a specified number of messages.")]
        public async Task Clean(
            [Summary("The number of messages to delete.")]
                int count)
            => await ModerationService.DeleteMessagesAsync(
                Context.Channel as ITextChannel, count, true,
                    () => ConfirmCleanAsync(Context.User.Id, $"in #{Context.Channel.Name}", count));

        [Command("clean")]
        [Summary("Mass-deletes a specified number of messages.")]
        public async Task Clean(
            [Summary("The number of messages to delete.")]
                int count,
            [Summary("The channel to clean.")]
                ITextChannel channel)
            => await ModerationService.DeleteMessagesAsync(
                channel, count, Context.Channel.Id == channel.Id,
                    () => ConfirmCleanAsync(Context.User.Id, $"in #{Context.Channel.Name}", count));

        [Command("clean")]
        [Summary("Mass-deletes a specified number of messages by the supplied user.")]
        public async Task Clean(
            [Summary("The number of messages to delete.")]
                int count,
            [Summary("The user whose messages should be deleted.")]
                IGuildUser user)
            => await ModerationService.DeleteMessagesAsync(
                Context.Channel as ITextChannel, user, count,
                    () => ConfirmCleanAsync(Context.User.Id, $"by {user.Nickname ?? $"{user.Username}#{user.Discriminator}"} in #{Context.Channel.Name}", count));

    internal protected IModerationService ModerationService { get; }

        private async Task<bool> ConfirmCleanAsync(ulong moderatorId, string messageOriginText, int count)
        {
            var confirmEmote = new Emoji("✅");
            var cancelEmote = new Emoji("❌");
            const int secondsToWait = 10;

            var cleanInfo = $"You are attempting to delete the past {count} messages {messageOriginText}.{Environment.NewLine}";

            var confirmationMessage = await ReplyAsync(cleanInfo +
                $"React with {confirmEmote} or {cancelEmote} in the next {secondsToWait} seconds to finalize or cancel the operation.");

            await confirmationMessage.AddReactionAsync(confirmEmote);
            await confirmationMessage.AddReactionAsync(cancelEmote);

            for (var i = 0; i < secondsToWait; i++)
            {
                await Task.Delay(1000);

                var cancelingUsers = await confirmationMessage.GetReactionUsersAsync(cancelEmote, int.MaxValue).FlattenAsync();
                if (cancelingUsers.Any(u => u.Id == moderatorId))
                {
                    await RemoveReactionsAndUpdateMessage("Cancellation was successfully received. Cancelling deletion.");
                    return false;
                }

                var confirmingUsers = await confirmationMessage.GetReactionUsersAsync(confirmEmote, int.MaxValue).FlattenAsync();
                if (confirmingUsers.Any(u => u.Id == moderatorId))
                {
                    await RemoveReactionsAndUpdateMessage("Confirmation was successfully received. Deleting messages.");
                    return true;
                }
            }

            await RemoveReactionsAndUpdateMessage("Confirmation was not received. Cancelling deletion.");
            return false;

            async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
            {
                await confirmationMessage.RemoveAllReactionsAsync();
                await confirmationMessage.ModifyAsync(m => m.Content = cleanInfo + bottomMessage);
            }
        }
    }
}
