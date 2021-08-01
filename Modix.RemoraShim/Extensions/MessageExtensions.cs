using System;
using System.Collections.Generic;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Remora.Discord.API.Abstractions.Objects
{
    public static class MessageExtensions
    {
        public static IPartialMessage ToPartialMessage(this IMessage message)
        {
            var msg = new PartialMessage
            {
                Activity = message.Activity,
                Application = message.Application,
                ApplicationID = message.ApplicationID,
                Attachments = new Optional<IReadOnlyList<IAttachment>>(message.Attachments),
                Author = new Optional<IUser>(message.Author),
                ChannelID = message.ChannelID,
                Components = message.Components,
                Content = message.Content,
                EditedTimestamp = message.EditedTimestamp,
                Embeds = new Optional<IReadOnlyList<IEmbed>>(message.Embeds),
                Flags = message.Flags,
                GuildID = message.GuildID,
                ID = message.ID,
                Interaction = message.Interaction,
                IsPinned = message.IsPinned,
                IsTTS = message.IsTTS,
                Member = message.Member,
                MentionedChannels = message.MentionedChannels,
                MentionedRoles = new Optional<IReadOnlyList<Snowflake>>(message.MentionedRoles),
                Mentions = new Optional<IReadOnlyList<IUserMention>>(message.Mentions),
                MentionsEveryone = message.MentionsEveryone,
                MessageReference = message.MessageReference,
                Nonce = message.Nonce,
                Reactions = message.Reactions,
                ReferencedMessage = message.ReferencedMessage,
                StickerItems = message.StickerItems,
                Thread = message.Thread,
                Timestamp = message.Timestamp,
                Type = message.Type,
                WebhookID = message.WebhookID
            };
            return msg;
        }
    }
}
