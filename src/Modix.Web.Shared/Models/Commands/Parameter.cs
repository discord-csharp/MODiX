namespace Modix.Web.Shared.Models.Commands;

public record Parameter(string Name, string Summary, IReadOnlyCollection<string> Options, string Type, bool IsOptional);
