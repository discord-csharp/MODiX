using System;
using System.Collections.Generic;
using System.Linq;

using Humanizer;

namespace Modix.Services.CommandHelp
{
    public class ModuleHelpData
    {
        public string Name { get; set; }

        public string Summary { get; set; }

        public IReadOnlyCollection<CommandHelpData> Commands { get; set; }

        public IReadOnlyCollection<string> HelpTags { get; set; }

        public static ModuleHelpData FromModuleInfo(Discord.Commands.ModuleInfo module)
        {
            return new()
            {
                Name = GetModuleHelpName(module.Name),
                Summary = string.IsNullOrWhiteSpace(module.Summary) ? "No summary" : module.Summary,
                Commands = module.Commands
                    .Where(x => !ShouldBeHidden(x))
                    .Select(x => CommandHelpData.FromCommandInfo(x))
                    .ToArray(),
                HelpTags = GetHelpTags(module.Attributes),
            };

            static bool ShouldBeHidden(Discord.Commands.CommandInfo command)
                => command.Preconditions.Any(x => x is Discord.Commands.RequireOwnerAttribute)
                || command.Attributes.Any(x => x is HiddenFromHelpAttribute);
        }

        public static ModuleHelpData FromModuleInfo(Discord.Interactions.ModuleInfo module)
        {
            var moduleHelp = module.Attributes.OfType<ModuleHelpAttribute>().SingleOrDefault();

            return new()
            {
                Name = GetModuleHelpName(moduleHelp?.Name ?? module.Name),
                Summary = !string.IsNullOrWhiteSpace(moduleHelp.Description) ? moduleHelp.Description
                    : !string.IsNullOrWhiteSpace(module.Description) ? module.Description : "No summary",
                Commands = module.SlashCommands
                    .Where(x => !ShouldBeHidden(x))
                    .Select(x => CommandHelpData.FromCommandInfo(x))
                    .ToArray(),
                HelpTags = GetHelpTags(module.Attributes),
            };

            static bool ShouldBeHidden(Discord.Interactions.SlashCommandInfo command)
                => command.Preconditions.Any(x => x is Discord.Interactions.RequireOwnerAttribute)
                || command.Attributes.Any(x => x is HiddenFromHelpAttribute);
        }

        private static string GetModuleHelpName(string moduleName)
        {
            var suffixPosition = moduleName.IndexOf("Module", StringComparison.Ordinal);
            if (suffixPosition > -1)
            {
                moduleName = moduleName[..suffixPosition].Humanize();
            }

            return moduleName.ApplyCase(LetterCasing.Title);
        }

        private static string[] GetHelpTags(IReadOnlyCollection<Attribute> attributes)
            => attributes
                .OfType<HelpTagsAttribute>()
                .SingleOrDefault()
                ?.Tags
                ?? Array.Empty<string>();
    }
}
