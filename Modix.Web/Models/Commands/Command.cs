using Modix.Services.CommandHelp;

namespace Modix.Web.Models.Commands;

public record Command(string Name, string Summary, IReadOnlyCollection<string> Aliases, IReadOnlyCollection<ParameterHelpData> Parameters, bool IsSlashCommand);
