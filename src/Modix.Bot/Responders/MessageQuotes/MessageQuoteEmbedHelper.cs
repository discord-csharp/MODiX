using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Humanizer.Bytes;
using Modix.Bot.Responders.AutoRemoveMessages;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.Utilities;

namespace Modix.Bot.Responders.MessageQuotes;

public class MessageQuoteEmbedHelper(AutoRemoveMessageService autoRemoveMessageService)
{
    public async Task BuildRemovableEmbed(IMessage message, IUser executingUser,
        Func<EmbedBuilder, Task<IUserMessage>> callback)
    {
        var embed = BuildQuoteEmbed(message, executingUser);

        if (callback is null || embed is null)
        {
            return;
        }

        await autoRemoveMessageService.RegisterRemovableMessageAsync(executingUser, embed,
            async (e) => await callback.Invoke(e));
    }

    public static EmbedBuilder BuildQuoteEmbed(IMessage message, IUser executingUser)
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
        {
            if (!TryAddImageEmbed(message, embed))
            {
                if (!TryAddThumbnailEmbed(message, embed))
                {
                    TryAddOtherAttachment(message, embed);
                }
            }
        }

        AddContent(message, embed);
        AddOtherEmbed(message, embed);
        AddActivity(message, embed);
        AddMeta(message, executingUser, embed);

        return embed;
    }

    private static bool TryAddImageAttachment(IMessage message, EmbedBuilder embed)
    {
        var firstAttachment = message.Attachments.FirstOrDefault();

        if (firstAttachment?.Height is null)
            return false;

        embed.WithImageUrl(firstAttachment.Url);

        return true;
    }

    private static void TryAddOtherAttachment(IMessage message, EmbedBuilder embed)
    {
        var firstAttachment = message.Attachments.FirstOrDefault();

        if (firstAttachment == null)
            return;

        embed.AddField($"Attachment (Size: {new ByteSize(firstAttachment.Size)})", firstAttachment.Url);
    }

    private static bool TryAddImageEmbed(IMessage message, EmbedBuilder embed)
    {
        var imageEmbed = message.Embeds.Select(x => x.Image).FirstOrDefault(x => x is { });

        if (imageEmbed is null)
            return false;

        embed.WithImageUrl(imageEmbed.Value.Url);

        return true;
    }

    private static bool TryAddThumbnailEmbed(IMessage message, EmbedBuilder embed)
    {
        var thumbnailEmbed = message.Embeds.Select(x => x.Thumbnail).FirstOrDefault(x => x is { });

        if (thumbnailEmbed is null)
            return false;

        embed.WithImageUrl(thumbnailEmbed.Value.Url);

        return true;
    }

    private static bool TryAddRichEmbed(IMessage message, IUser executingUser, ref EmbedBuilder embed)
    {
        var firstEmbed = message.Embeds.FirstOrDefault();

        if (firstEmbed?.Type != EmbedType.Rich)
            return false;

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

    private static void AddActivity(IMessage message, EmbedBuilder embed)
    {
        if (message.Activity == null) return;

        embed
            .AddField("Invite Type", message.Activity.Type)
            .AddField("Party Id", message.Activity.PartyId);
    }

    private static void AddOtherEmbed(IMessage message, EmbedBuilder embed)
    {
        if (message.Embeds.Count == 0) return;

        embed.AddField("Embed Type", message.Embeds.First().Type);
    }

    private static void AddContent(IMessage message, EmbedBuilder embed)
    {
        if (string.IsNullOrWhiteSpace(message.Content)) return;

        embed.WithDescription(message.Content);
    }

    private static void AddMeta(IMessage message, IUser executingUser, EmbedBuilder embed)
    {
        embed
            .WithUserAsAuthor(message.Author)
            .WithTimestamp(message.Timestamp)
            .WithColor(new Color(95, 186, 125))
            .AddField("Quoted by", $"{executingUser.Mention} from **{message.GetJumpUrlForEmbed()}**", true);
    }

    private static bool IsQuote(IMessage message)
    {
        return message
            .Embeds?
            .SelectMany(d => d.Fields)
            .Any(d => d.Name == "Quoted by") == true;
    }
}
