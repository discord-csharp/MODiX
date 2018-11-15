using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace Modix.Services.Moderation
{
    public class AttachmentBlacklistBehavior : BehaviorBase
    {
        public static readonly IReadOnlyCollection<string> BlacklistedExtensions = new[]
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
            ".cmd",
            ".vbs",
            ".js",
            ".reg",
            ".pif",
            ".msp",
            ".hta",
            ".cpl",
            ".jar",
            ".vbe",
            ".ws",
            ".wsf",
            ".wsc",
            ".wsh",
            ".ps1",
            ".ps1xml",
            ".ps2",
            ".ps2xml",
            ".psc1",
            ".pasc2",
            ".msh",
            ".msh1",
            ".msh2",
            ".mshxml",
            ".msh1xml",
            ".msh2xml",
            ".scf",
            ".lnk",
            ".inf",
            ".doc",
            ".xls",
            ".ppt",
            ".docm",
            ".dotm",
            ".xlsm",
            ".xltm",
            ".xlam",
            ".pptm",
            ".potm",
            ".ppam",
            ".ppsm",
            ".sldn"
        };

        private DiscordSocketClient DiscordClient { get; }

        public AttachmentBlacklistBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            DiscordClient = discordClient;
        }

        private async Task Handle(IMessage message)
        {
            if (!message.Attachments.Any())
                return;

            if (!(message is SocketUserMessage userMessage) || !(userMessage.Author is SocketGuildUser || userMessage.Author.IsBot))
                return;

            // Check if the attachment's file name ends in anything suspicious first
            if (!BlacklistedExtensions.Any(ext => message.Attachments.Any(m => m.Filename.ToLower().EndsWith(ext))))
                return;

            await TryDeleteAndReplyToUser(message);
        }

        private async Task TryDeleteAndReplyToUser(IMessage message)
        {
            try
            {
                var attachments = string.Join(", ", message.Attachments.Select(x => x.Filename));

                await SelfExecuteRequest<IModerationService>(async moderationService =>
                {
                    await moderationService.DeleteMessageAsync(message, $"Message had suspicious files attached: {attachments}");
                });

                var reply = GetReplyToUser(message);
                await message.Channel.SendMessageAsync(reply);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Failed to remove message {messageId} with suspicious file(s) attached in {channelName}", message.Id, message.Channel.Name);
            }
        }

        private static string GetReplyToUser(IMessage message)
            => $"Please don't upload any potentially harmful files {message.Author.Mention}, your message has been removed";

        internal protected override Task OnStartingAsync()
        {
            DiscordClient.MessageReceived += Handle;
            return Task.CompletedTask;
        }

        internal protected override Task OnStoppedAsync()
        {
            DiscordClient.MessageReceived -= Handle;
            return Task.CompletedTask;
        }
    }
}
