using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;

namespace Modix.Services.CommandHelp
{
    public class ModuleHelpData
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<CommandHelpData> Commands { get; set; } = new List<CommandHelpData>();

        public static ModuleHelpData FromModuleInfo(ModuleInfo module)
        {
            var ret = new ModuleHelpData
            {
                Name = module.Name,
                Summary = module.Summary
            };

            foreach (var command in module.Commands)
            {
                if (command.Preconditions.Any(precon => precon is RequireOwnerAttribute) ||
                    command.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
                {
                    continue;
                }

                ret.Commands.Add(CommandHelpData.FromCommandInfo(command));
            }

            return ret;
        }
    }
}
