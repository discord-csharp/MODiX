namespace Modix.Web.Shared.Models.Common;

public record ChannelInformation(ulong Id, string Name) : IAutoCompleteItem;
