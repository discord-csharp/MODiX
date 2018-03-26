using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace Modix.Services.CommandHelp
{
    public class CommandHelpData
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<ParameterHelpData> Parameters { get; set; } = new List<ParameterHelpData>();

        public static CommandHelpData FromCommandInfo(CommandInfo command)
        {
            var ret = new CommandHelpData
            {
                Name = command.Name,
                Summary = command.Summary
            };

            foreach (var param in command.Parameters)
            {
                ret.Parameters.Add(ParameterHelpData.FromParameterInfo(param));
            }

            return ret;
        }
    }
}
