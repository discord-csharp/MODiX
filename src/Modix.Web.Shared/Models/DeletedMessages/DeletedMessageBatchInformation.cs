namespace Modix.Web.Shared.Models.DeletedMessages;

public record DeletedMessageBatchInformation
(
    string ChannelName,
    string AuthorUsername,
    DateTimeOffset Created,
    string CreatedByUsername,
    string Content,
    string Reason,
    long? BatchId
);
