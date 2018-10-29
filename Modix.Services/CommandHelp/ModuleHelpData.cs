using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Humanizer;

namespace Modix.Services.CommandHelp
{
    public class ModuleHelpData
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<CommandHelpData> Commands { get; set; } = new List<CommandHelpData>();

        public static ModuleHelpData FromModuleInfo(ModuleInfo module)
        {
            string moduleName = module.Name;

            int suffixPosition = moduleName.IndexOf("Module");
            if (suffixPosition > -1)
            {
                moduleName = module.Name.Substring(0, suffixPosition).Humanize();
            }

            var ret = new ModuleHelpData
            {
                Name = moduleName,
                Summary = string.IsNullOrWhiteSpace(module.Summary) ? "No Summary" : module.Summary
            };

            foreach (var command in module.Commands)
            {
                if (command.Preconditions.Any(precon => precon is RequireOwnerAttribute) ||
                    command.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
                {
                    continue;
                }

                ret.Commands.AddRange(CommandHelpData.FromCommandInfo(command));
            }

            return ret;
        }
    }
}
