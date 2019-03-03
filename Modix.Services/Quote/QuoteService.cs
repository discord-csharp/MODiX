using System;
using System.Linq;
using Discord;
using Humanizer.Bytes;
using static System.FormattableString;

namespace Modix.Services.Quote
{
    public interface IQuoteService
    {
        /// <summary>
        /// Build an embed quote for the given message. Returns null if the message could not be quoted.
        /// </summary>
        /// <param name="message">The message to quote</param>
        /// <param name="executingUser">The user that is doing the quoting</param>
        EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser);
    }

    public class QuoteService : IQuoteService
    {
        /// <inheritdoc />
        public EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser)
        {
            var embed = new EmbedBuilder();

            if (TryAddRichEmbed(message, executingUser, ref embed))
            {
                return embed;
            }

            if (TryAddImageAttachment(message, executingUser, ref embed) == false)
            {
                TryAddOtherAttachment(message, executingUser, ref embed);
            }

            AddContent(message, executingUser, ref embed);
            AddOtherEmbed(message, executingUser, ref embed);
            AddActivity(message, executingUser, ref embed);
            AddMeta(message, executingUser, ref embed);

            return embed;
        }

        private bool TryAddImageAttachment(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null || firstAttachment.Height == null) { return false; }

            embed = embed
                .WithImageUrl(firstAttachment.Url);

            return true;
        }

        private bool TryAddOtherAttachment(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null) { return false; }

            embed = embed
                .AddField($"Attachment (Size: {new ByteSize(firstAttachment.Size)})", firstAttachment.Url);

            return true;
        }

        private bool TryAddRichEmbed(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            var firstEmbed = message.Embeds.FirstOrDefault();
            if (firstEmbed?.Type != EmbedType.Rich) { return false; }

            embed = message.Embeds
                    .First()
                    .ToEmbedBuilder()
                    .AddField("Quoted by", executingUser.Mention, true);

            if (firstEmbed.Color == null)
            {
                embed.Color = Color.DarkGrey;
            }

            return true;
        }

        private void AddActivity(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            if (message.Activity == null) { return; }

            embed = embed
                .AddField("Invite Type", message.Activity.Type)
                .AddField("Party Id", message.Activity.PartyId);
        }

        private void AddOtherEmbed(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            if (!message.Embeds.Any()) { return; }

            embed = embed
                .AddField("Embed Type", message.Embeds.First().Type);
        }

        private void AddContent(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) { return; }

            string messageUrl = null;
            if (message.Channel is IGuildChannel guildChannel && guildChannel.Guild is IGuild guild)
            {
                messageUrl = Invariant($"https://discordapp.com/channels/{guild.Id}/{guildChannel.Id}/{message.Id}");
            }

            embed = embed
                .WithDescription(message.Content);
        }

        private void AddMeta(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            embed = embed
                .WithAuthor(message.Author)
                .WithFooter(GetPostedMeta(message))
                .WithColor(new Color(95, 186, 125))
                .AddField("Quoted by", $"{executingUser.Mention} from **[#{message.Channel.Name}]({message.GetJumpUrl()})**", true);
        }

        private static string GetPostedMeta(IMessage message)
            => string.Format("{0:dddd, dd}{1} {0:MMMM yyyy} at {0:HH:mm}, in #{2}", message.Timestamp, GetDaySuffix(message.Timestamp.Day), message.Channel.Name);

        private static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}
