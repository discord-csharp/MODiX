namespace Modix.Web.Models.Common;

public record ChannelInformation(ulong Id, string Name) : IAutoCompleteItem;
