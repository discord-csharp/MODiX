using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace Modix.Services.FileUpload
{
    public class FileUploadHandler
    {
        private const ulong ChannelIdToPostModerationLog = 360507591488438283;

        private readonly List<string> _blacklistedExtensions = new List<string>
        {
            ".exe",
            ".dll",
            ".application",
            ".msc",
            ".bat",
            ".pdb",
            ".sh",
            ".com",
            ".scr",
            ".msi",
            ".cmd"
        };

        public async Task Handle(IMessage message)
        {
            // Check if the attachment's file name ends in anything suspicious first
            if (!_blacklistedExtensions.Any(ext => message.Attachments.Any(m => m.Filename.EndsWith(ext))))
                return;

            await TryPostToModerationChannel(message);

            await TryDeleteAndReplyToUser(message);
        }

        private static async Task TryPostToModerationChannel(IMessage message)
        {
            try
            {
                if (!(message.Channel is SocketTextChannel channel))
                    return;

                var moderationChannel = channel.Guild.Channels.SingleOrDefault(x => x.Id == ChannelIdToPostModerationLog) as SocketTextChannel;

                if (moderationChannel == null)
                {
                    Log.Debug("Moderation channel with ID: {id} not found", ChannelIdToPostModerationLog);
                    return;
                }

                var moderationEmbed = BuildModerationEmbed(message);
                await moderationChannel.SendMessageAsync(string.Empty, false, moderationEmbed);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Failed posting to moderation channel {channelId}", ChannelIdToPostModerationLog);
            }
        }

        private static async Task TryDeleteAndReplyToUser(IMessage message)
        {
            try
            {
                await message.DeleteAsync();

                var reply = GetReplyToUser(message);

                await message.Channel.SendMessageAsync(reply);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to remove message {messageId} with suspicious file(s) attached in {channelName}", message.Id, message.Channel.Name);
            }
        }

        private static EmbedBuilder BuildModerationEmbed(IMessage message)
        {
            var embed = new EmbedBuilder().WithTitle("Message with suspicious file(s) removed");

            embed.AddField(a => a.WithName("Message ID").WithValue(message.Id));

            if (!string.IsNullOrWhiteSpace(message.Content))
                embed.AddField(a => a.WithName("Content").WithValue(message.Content));

            var files = string.Join(", ", message.Attachments.Select(x => x.Filename));

            embed.AddField(a => a.WithName("Files").WithValue($"{message.Attachments.Count} file(s) attached: {files}"));

            embed.AddField(a => a.WithName("Time").WithValue($"{message.Timestamp:yyyy/MM/dd HH:mm}"));

            embed.AddField(a => a.WithName("Channel").WithValue($"#{message.Channel.Name}"));

            embed.AddField(a => a.WithName("Author").WithValue(message.Author.Mention));

            embed.WithColor(new Color(255, 51, 51));

            return embed;
        }

        private static string GetReplyToUser(IMessage message)
            => $"Please don't upload any potentially harmful files {message.Author.Mention}, your message has been removed";
    }
}
