using System.Collections.Generic;
using System.Linq;

namespace Modix.Services.CommandHelp
{
    public class CommandHelpData
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public IReadOnlyCollection<string> Aliases { get; set; }

        public IReadOnlyCollection<ParameterHelpData> Parameters { get; set; }

        public bool IsSlashCommand { get; set; }

        public static CommandHelpData FromCommandInfo(Discord.Commands.CommandInfo command)
            => new()
            {
                Name = command.Name,
                Summary = command.Summary,
                Aliases = command.Aliases,
                Parameters = command.Parameters
                        .Select(x => ParameterHelpData.FromParameterInfo(x))
                        .ToArray(),
            };

        public static CommandHelpData FromCommandInfo(Discord.Interactions.SlashCommandInfo command)
            => new()
            {
                Name = command.ToString(),
                Summary = command.Description,
                Aliases = new[] { command.ToString() },
                Parameters = command.Parameters
                        .Select(x => ParameterHelpData.FromParameterInfo(x))
                        .ToArray(),
                IsSlashCommand = true,
            };
    }
}
