using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Humanizer.Bytes;
using Modix.Services.AutoRemoveMessage;

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

        Task BuildRemovableEmbed(IMessage message, IUser executingUser, Func<EmbedBuilder, Task<IUserMessage>> callback);
    }

    public class QuoteService : IQuoteService
    {
        private readonly IAutoRemoveMessageService _autoRemoveMessageService;

        public QuoteService(IAutoRemoveMessageService autoRemoveMessageService)
        {
            _autoRemoveMessageService = autoRemoveMessageService;
        }

        /// <inheritdoc />
        public EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser)
        {
            if (IsQuote(message))
            {
                return null;
            }

            var embed = new EmbedBuilder();
            if (TryAddRichEmbed(message, executingUser, ref embed))
            {
                return embed;
            }

            if (!TryAddImageAttachment(message, ref embed))
            {
                TryAddOtherAttachment(message, ref embed);
            }

            AddContent(message, ref embed);
            AddOtherEmbed(message, ref embed);
            AddActivity(message, ref embed);
            AddMeta(message, executingUser, ref embed);

            return embed;
        }

        public async Task BuildRemovableEmbed(IMessage message, IUser executingUser, Func<EmbedBuilder, Task<IUserMessage>> callback)
        {
            var embed = BuildQuoteEmbed(message, executingUser);

            if(callback == null || embed == null)
            {
                return;
            }

            await _autoRemoveMessageService.RegisterRemovableMessageAsync(executingUser, embed,
                async (e) => await callback.Invoke(e));
        }

        private bool TryAddImageAttachment(IMessage message, ref EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null || firstAttachment.Height == null)
                return false;

            embed = embed
                .WithImageUrl(firstAttachment.Url);

            return true;
        }

        private bool TryAddOtherAttachment(IMessage message, ref EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null) return false;

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
                    .AddField("Quoted by", $"{executingUser.Mention} from **[#{message.Channel.Name}]({message.GetJumpUrl()})**", true);

            if (firstEmbed.Color == null)
            {
                embed.Color = Color.DarkGrey;
            }

            return true;
        }

        private void AddActivity(IMessage message, ref EmbedBuilder embed)
        {
            if (message.Activity == null) { return; }

            embed = embed
                .AddField("Invite Type", message.Activity.Type)
                .AddField("Party Id", message.Activity.PartyId);
        }

        private void AddOtherEmbed(IMessage message, ref EmbedBuilder embed)
        {
            if (message.Embeds.Count == 0) return;

            embed = embed
                .AddField("Embed Type", message.Embeds.First().Type);
        }

        private void AddContent(IMessage message, ref EmbedBuilder embed)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            embed = embed.WithDescription(message.Content);
        }

        private void AddMeta(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            embed = embed
                .WithAuthor(message.Author)
                .WithTimestamp(message.Timestamp)
                .WithColor(new Color(95, 186, 125))
                .AddField("Quoted by", $"{executingUser.Mention} from **[#{message.Channel.Name}]({message.GetJumpUrl()})**", true);
        }

        private bool IsQuote(IMessage message)
        {
            return message
                .Embeds?
                .SelectMany(d => d.Fields)
                .Any(d => d.Name == "Quoted by") == true;
        }
    }
}
