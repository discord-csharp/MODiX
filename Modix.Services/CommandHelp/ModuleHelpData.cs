using System;
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

        public IReadOnlyCollection<CommandHelpData> Commands { get; set; }

        public static ModuleHelpData FromModuleInfo(ModuleInfo module)
        {
            var moduleName = module.Name;

            var suffixPosition = moduleName.IndexOf("Module", StringComparison.Ordinal);
            if (suffixPosition > -1)
            {
                moduleName = module.Name.Substring(0, suffixPosition).Humanize();
            }

            moduleName = moduleName.ApplyCase(LetterCasing.Title);

            var ret = new ModuleHelpData
            {
                Name = moduleName,
                Summary = string.IsNullOrWhiteSpace(module.Summary) ? "No Summary" : module.Summary,
                Commands = module.Commands
                    .Where(x => !ShouldBeHidden(x))
                    .Select(x => CommandHelpData.FromCommandInfo(x))
                    .ToArray(),
            };

            return ret;

            bool ShouldBeHidden(CommandInfo command)
                => command.Preconditions.Any(x => x is RequireOwnerAttribute)
                || command.Attributes.Any(x => x is HiddenFromHelpAttribute);
        }
    }
}
