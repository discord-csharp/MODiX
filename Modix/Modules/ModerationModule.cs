using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

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

        [Command("moderation muterole get")]
        [Summary("Retrieves the role currently configured for use by the \"mute\" command")]
        public async Task GetMuteRole()
        {
            var muteRole = await ModerationService.GetMuteRoleAsync(Context.Guild);

            await ReplyAsync(Format.Code(muteRole.Name));
        }

        [Command("moderation muterole set")]
        [Summary("Changes the role currently configured for use by the \"mute\" command")]
        public Task SetMuteRole(
            [Summary("The role to be used by the \"mute\" command")]
                IRole muteRole)
            => ModerationService.SetMuteRoleAsync(Context.Guild, muteRole);

        [Command("moderation logchannels")]
        [Summary("Lists all channels currently configured to receive moderation log messages")]
        public async Task GetLogChannels()
        {
            var logChannels = await ModerationService.GetLogChannelsAsync(Context.Guild);

            await ReplyAsync(logChannels.Any()
                ? string.Join("\r\n",
                    logChannels.Select(x => $"#{x.Name}"))
                : "There are no moderation log channels currently configured.");
        }

        [Command("moderation logchannels add")]
        [Summary("Configures a channel to received moderation log messages")]
        public Task AddLogChannel(
            [Summary("The channel to receive log messages")]
                IMessageChannel channel)
            => ModerationService.AddLogChannelAsync(Context.Guild, channel);

        [Command("moderation logchannels remove")]
        [Summary("Configures a channel to stop receiving moderation log messages")]
        public Task RemoveLogChannel(
            [Summary("The channel to stop receiving log messages")]
                IMessageChannel channel)
            => ModerationService.RemoveLogChannelAsync(Context.Guild, channel);

        [Command("note")]
        [Summary("Applies a note to a user's infraction history.")]
        public Task Note(
            [Summary("The user to which the note is being applied.")]
                IGuildUser subject,
            [Summary("The reason for the note.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Notice, subject.Id, reason, null);

        [Command("warn")]
        [Summary("Issue a warning to a user.")]
        public Task Warn(
            [Summary("The user to which the warning is being issued.")]
                IGuildUser subject,
            [Summary("The reason for the warning.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Warning, subject.Id, reason, null);

        [Command("mute")]
        [Summary("Mute a user.")]
        public Task Mute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, null);

        [Command("tempmute")]
        [Summary("Mute a user, for a temporary amount of time.")]
        public Task TempMute(
            [Summary("The user to be muted.")]
                IGuildUser subject,
            [Summary("The duration of the mute.")]
                string durationString,
            [Summary("The reason for the mute.")]
            [Remainder]
                string reason)
        {
            // TODO: Remove when we port to 2.0
            var duration = TimeSpanTypeReader.Read(durationString);
            if (!duration.HasValue) { throw new ArgumentException("Invalid Timespan Format"); }

            return ModerationService.CreateInfractionAsync(InfractionType.Mute, subject.Id, reason, duration.Value);
        }

        [Command("unmute")]
        [Summary("Remove a mute that has been applied to a user.")]
        public Task UnMute(
            [Summary("The user to be un-muted.")]
                IGuildUser subject)
            => ModerationService.RescindInfractionAsync(InfractionType.Mute, subject.Id);

        [Command("ban")]
        [Summary("Ban a user from the current guild.")]
        public Task Ban(
            [Summary("The user to be banned.")]
                IGuildUser subject,
            [Summary("The reason for the ban.")]
            [Remainder]
                string reason)
            => ModerationService.CreateInfractionAsync(InfractionType.Ban, subject.Id, reason, null);

        [Command("unban")]
        [Summary("Remove a ban that has been applied to a user.")]
        public Task UnBan(
            [Summary("The user to be un-banned.")]
                IGuildUser subject)
            => ModerationService.RescindInfractionAsync(InfractionType.Ban, subject.Id);

        internal protected IModerationService ModerationService { get; }
    }
}
