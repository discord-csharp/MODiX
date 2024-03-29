namespace Modix.Web.Models.Commands;

public record Module(string Name, string Summary, IEnumerable<Command> Commands);
