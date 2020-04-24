using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Humanizer.Bytes;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Utilities;

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

            if (message.Attachments.Any(x => x.IsSpoiler())
                || message.Embeds.Any() && FormatUtilities.ContainsSpoiler(message.Content))
            {
                embed.AddField("Spoiler warning", "The quoted message contains spoilered content.");
            }
            else if (!TryAddImageAttachment(message, embed))
                if (!TryAddImageEmbed(message, embed))
                    if (!TryAddThumbnailEmbed(message, embed))
                        TryAddOtherAttachment(message, embed);

            AddContent(message, embed);
            AddOtherEmbed(message, embed);
            AddActivity(message, embed);
            AddMeta(message, executingUser, embed);

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

        private bool TryAddImageAttachment(IMessage message, EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null || firstAttachment.Height == null)
                return false;

            embed.WithImageUrl(firstAttachment.Url);

            return true;
        }

        private bool TryAddOtherAttachment(IMessage message, EmbedBuilder embed)
        {
            var firstAttachment = message.Attachments.FirstOrDefault();
            if (firstAttachment == null) return false;

            embed.AddField($"Attachment (Size: {new ByteSize(firstAttachment.Size)})", firstAttachment.Url);

            return true;
        }

        private bool TryAddImageEmbed(IMessage message, EmbedBuilder embed)
        {
            var imageEmbed = message.Embeds.Select(x => x.Image).FirstOrDefault(x => x is { });
            if (imageEmbed is null)
                return false;

            embed.WithImageUrl(imageEmbed.Value.Url);

            return true;
        }

        private bool TryAddThumbnailEmbed(IMessage message, EmbedBuilder embed)
        {
            var thumbnailEmbed = message.Embeds.Select(x => x.Thumbnail).FirstOrDefault(x => x is { });
            if (thumbnailEmbed is null)
                return false;

            embed.WithImageUrl(thumbnailEmbed.Value.Url);

            return true;
        }

        private bool TryAddRichEmbed(IMessage message, IUser executingUser, ref EmbedBuilder embed)
        {
            var firstEmbed = message.Embeds.FirstOrDefault();
            if (firstEmbed?.Type != EmbedType.Rich) { return false; }

            embed = message.Embeds
                    .First()
                    .ToEmbedBuilder()
                    .AddField("Quoted by", $"{executingUser.Mention} from **{message.GetJumpUrlForEmbed()}**", true);

            if (firstEmbed.Color == null)
            {
                embed.Color = Color.DarkGrey;
            }

            return true;
        }

        private void AddActivity(IMessage message, EmbedBuilder embed)
        {
            if (message.Activity == null) { return; }

            embed
                .AddField("Invite Type", message.Activity.Type)
                .AddField("Party Id", message.Activity.PartyId);
        }

        private void AddOtherEmbed(IMessage message, EmbedBuilder embed)
        {
            if (message.Embeds.Count == 0) return;

            embed.AddField("Embed Type", message.Embeds.First().Type);
        }

        private void AddContent(IMessage message, EmbedBuilder embed)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            embed.WithDescription(message.Content);
        }

        private void AddMeta(IMessage message, IUser executingUser, EmbedBuilder embed)
        {
            embed
                .WithUserAsAuthor(message.Author)
                .WithTimestamp(message.Timestamp)
                .WithColor(new Color(95, 186, 125))
                .AddField("Quoted by", $"{executingUser.Mention} from **{message.GetJumpUrlForEmbed()}**", true);
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
