﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Humanizer.Bytes;
using Modix.Data.Models.Moderation;

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
            string content = msg.Content;

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

            var ret = new DeletedMessageAbstraction
            {
                MessageId = msg.Id,
                Username = $"{msg.Author.Username}#{msg.Author.Discriminator}",
                Content = msg.Content,
                SentTime = msg.CreatedAt,
                Url = msg.GetJumpUrl()
            };

            return ret;
        }

        public static DeletedMessageAbstraction FromSummary(DeletedMessageSummary msg)
        {
            var ret = new DeletedMessageAbstraction
            {
                MessageId = msg.MessageId,
                Username = $"{msg.Author.Username}#{msg.Author.Discriminator}",
                Content = msg.Content,
                SentTime = null,
                Url = null
            };

            return ret;
        }
    }
}
