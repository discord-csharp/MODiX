using System;
using System.Linq;
using Discord;
using Humanizer.Bytes;
using Modix.Data.Models.Moderation;
using Modix.Services.Utilities;

namespace Modix.Models
{
    public class DeletedMessageAbstraction
    {
        public ulong MessageId { get; set; }
        public string Username { get; set; }
        public DateTimeOffset? SentTime { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }

        public static DeletedMessageAbstraction FromIMessage(IMessage msg)
        {
            var content = msg.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                if (msg.Embeds.Any())
                {
                    content = $"Embed: {msg.Embeds.First().Title}: {msg.Embeds.First().Description}";
                }
                else if (msg.Attachments.Any())
                {
                    content = $"Attachment: {msg.Attachments.First().Filename} {ByteSize.FromBytes(msg.Attachments.First().Size)}";
                }
            }

            return new DeletedMessageAbstraction
            {
                MessageId = msg.Id,
                Username = msg.Author.GetFullUsername(),
                Content = content,
                SentTime = msg.CreatedAt,
                Url = msg.GetJumpUrl()
            };
        }

        public static DeletedMessageAbstraction FromSummary(DeletedMessageSummary msg)
        {
            return new DeletedMessageAbstraction
            {
                MessageId = msg.MessageId,
                Username = msg.Author.GetFullUsername(),
                Content = msg.Content,
                SentTime = null,
                Url = null
            };
        }
    }
}
