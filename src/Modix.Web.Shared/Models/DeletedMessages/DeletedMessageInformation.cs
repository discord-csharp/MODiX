namespace Modix.Web.Shared.Models.DeletedMessages;

public record DeletedMessageInformation(ulong MessageId, DateTimeOffset? SentTime, string? Url, string Username, string Content);
