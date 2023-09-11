using Discord;
using Humanizer.Bytes;
using Modix.Services.Utilities;

namespace Modix.Web.Models.DeletedMessages;

public record DeletedMessageInformation(ulong MessageId, DateTimeOffset? SentTime, string? Url, string Username, string Content)
{
    public static DeletedMessageInformation FromIMessage(IMessage message)
    {
        var content = message.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            if (message.Embeds.Count > 0)
            {
                content = $"Embed: {message.Embeds.First().Title}: {message.Embeds.First().Description}";
            }
            else if (message.Attachments.Count > 0)
            {
                content = $"Attachment: {message.Attachments.First().Filename} {ByteSize.FromBytes(message.Attachments.First().Size)}";
            }
        }

        return new DeletedMessageInformation(message.Id, message.CreatedAt, message.GetJumpUrl(), message.Author.GetDisplayName(), content);
    }
}
