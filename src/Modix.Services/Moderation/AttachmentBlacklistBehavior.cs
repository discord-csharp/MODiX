#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Services.Core;
using Modix.Data.Models.Core;

namespace Modix.Services.Moderation
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class AttachmentBlacklistBehavior
        : INotificationHandler<MessageReceivedNotification>
    {
        /// <summary>
        /// Gets the set of blacklisted extensions.
        /// </summary>
        /// <remarks>
        /// When adding new extensions, maintain the alphabetical order to improve readability.
        /// </remarks>
        public static readonly ImmutableHashSet<string> BlacklistedExtensions =
        [
               ".application",
               ".bat",
               ".bin",
               ".cmd",
               ".com",
               ".cpl",
               ".dll",
               ".doc",
               ".docm",
               ".dotm",
               ".exe",
               ".gadget",
               ".hta",
               ".inf",
               ".inf1",
               ".ins",
               ".inx",
               ".isu",
               ".jar",
               ".job",
               ".js",
               ".jse",
               ".lnk",
               ".msc",
               ".msh",
               ".msh1",
               ".msh1xml",
               ".msh2",
               ".msh2xml",
               ".mshxml",
               ".msi",
               ".msp",
               ".paf",
               ".pasc2",
               ".pdb",
               ".pdf",
               ".pif",
               ".potm",
               ".ppam",
               ".ppsm",
               ".ppt",
               ".pptm",
               ".ps1",
               ".ps1xml",
               ".ps2",
               ".ps2xml",
               ".psc1",
               ".reg",
               ".rgs",
               ".sb",
               ".scf",
               ".scr",
               ".sct",
               ".sh",
               ".shb",
               ".shs",
               ".sldn",
               ".u3p",
               ".vbe",
               ".vbs",
               ".vbscript",
               ".ws",
               ".wsc",
               ".wsf",
               ".wsh",
               ".xlam",
               ".xls",
               ".xlsm",
               ".xltm",
               ".zip",
        ];

        public AttachmentBlacklistBehavior(
            DesignatedChannelService designatedChannelService,
            DiscordSocketClient discordSocketClient,
            ILogger<AttachmentBlacklistBehavior> logger,
            ModerationService moderationService)
        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _logger = logger;
            _moderationService = moderationService;
        }

        public async Task HandleNotificationAsync(
            MessageReceivedNotification notification,
            CancellationToken cancellationToken)
        {
            var message = notification.Message;
            var channel = notification.Message.Channel;
            var guild = (channel as SocketGuildChannel)?.Guild;
            var author = notification.Message.Author;

            using var logScope = AttachmentBlacklistLogMessages.BeginMessageScope(_logger, guild?.Id, channel.Id, author.Id, message.Id);

            if (!message.Attachments.Any())
            {
                AttachmentBlacklistLogMessages.IgnoringMessageWithNoAttachments(_logger);
                return;
            }

            if (guild is null)
            {
                AttachmentBlacklistLogMessages.IngoringNonGuildMessage(_logger);
                return;
            }

            if (author.IsBot || author.IsWebhook)
            {
                AttachmentBlacklistLogMessages.IngoringNonHumanMessage(_logger);
                return;
            }

            AttachmentBlacklistLogMessages.ChannelModerationStatusFetching(_logger);
            var channelIsUnmoderated = await _designatedChannelService.ChannelHasDesignation(
                guild.Id,
                channel.Id,
                DesignatedChannelType.Unmoderated,
                cancellationToken);
            AttachmentBlacklistLogMessages.ChannelModerationStatusFetched(_logger);
            if (channelIsUnmoderated)
            {
                AttachmentBlacklistLogMessages.IgnoringUnmoderatedChannel(_logger);
                return;
            }

            AttachmentBlacklistLogMessages.SuspiciousAttachmentsSearching(_logger);
            var blacklistedFilenames = message.Attachments
                .Select(attachment => attachment.Filename.ToLower())
                .Where(filename => BlacklistedExtensions.Contains(Path.GetExtension(filename)))
                .ToArray();

            if(!blacklistedFilenames.Any())
            {
                AttachmentBlacklistLogMessages.SuspiciousAttachmentsNotFound(_logger);
                return;
            }
            AttachmentBlacklistLogMessages.SuspiciousAttachmentsFound(_logger, blacklistedFilenames.Length);

            var selfUser = _discordSocketClient.CurrentUser;
            AttachmentBlacklistLogMessages.SelfUserFetched(_logger, selfUser.Id);

            AttachmentBlacklistLogMessages.SuspiciousMessageDeleting(_logger);
            await _moderationService.DeleteMessageAsync(
                message,
                $"Message had suspicious files attached: {string.Join(", ", blacklistedFilenames)}",
                selfUser.Id,
                cancellationToken);
            AttachmentBlacklistLogMessages.SuspiciousMessageDeleted(_logger);

            AttachmentBlacklistLogMessages.ReplySending(_logger);
            await message.Channel.SendMessageAsync(
                $"Please don't upload any potentially harmful files {author.Mention}, your message has been removed",
                options: new RequestOptions() { CancelToken = cancellationToken });
            AttachmentBlacklistLogMessages.ReplySent(_logger);
        }

        private readonly DesignatedChannelService _designatedChannelService;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger _logger;
        private readonly ModerationService _moderationService;
    }
}
