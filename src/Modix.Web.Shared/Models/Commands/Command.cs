namespace Modix.Web.Shared.Models.Commands;

public record Command(string Name, string Summary, IReadOnlyCollection<string> Aliases, IReadOnlyCollection<Parameter> Parameters, bool IsSlashCommand);
