using System.Collections.Generic;
using Discord.Commands;

namespace Modix.Services.CommandHelp
{
    public class CommandHelpData
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public List<ParameterHelpData> Parameters { get; set; } = new List<ParameterHelpData>();

        public static IEnumerable<CommandHelpData> FromCommandInfo(CommandInfo command)
        {
            List<CommandHelpData> ret = new List<CommandHelpData>();

            foreach (var alias in command.Aliases)
            {
                var data = new CommandHelpData
                {
                    Alias = alias,
                    Name = command.Name,
                    Summary = command.Summary
                };

                foreach (var param in command.Parameters)
                {
                    data.Parameters.Add(ParameterHelpData.FromParameterInfo(param));
                }

                ret.Add(data);
            }

            return ret;
        }
    }
}
