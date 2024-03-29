using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord.Commands;
using Discord.Interactions;

namespace Modix.Services.CommandHelp
{
    /// <summary>
    /// Provides functionality to retrieve command help information.
    /// </summary>
    public interface ICommandHelpService
    {
        /// <summary>
        /// Retrieves help data for all available modules.
        /// </summary>
        /// <returns>
        /// A readonly collection of data about all available modules.
        /// </returns>
        IReadOnlyCollection<ModuleHelpData> GetModuleHelpData();

        /// <summary>
        /// Retrieves module help data for the supplied query.
        /// </summary>
        /// <param name="query">A query to use to search for an applicable help module.</param>
        /// <returns>
        /// Help information for the supplied query, or <see langword="null"/> if no information could be found for the supplied query.
        /// </returns>
        ModuleHelpData GetModuleHelpData(string query);

        /// <summary>
        /// Retrieves command help data for the supplied query.
        /// </summary>
        /// <param name="query">A query to use to search for an applicable help module.</param>
        /// <returns>
        /// Help information for the supplied query, or <see langword="null"/> if no information could be found for the supplied query.
        /// </returns>
        CommandHelpData GetCommandHelpData(string query);
    }

    /// <inheritdoc />
    internal class CommandHelpService : ICommandHelpService
    {
        private readonly CommandService _commandService;
        private readonly InteractionService _interactionService;
        private IReadOnlyCollection<ModuleHelpData> _cachedHelpData;

        public CommandHelpService(CommandService commandService, InteractionService interactionService)
        {
            _commandService = commandService;
            _interactionService = interactionService;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ModuleHelpData> GetModuleHelpData()
            => LazyInitializer.EnsureInitialized(ref _cachedHelpData, () =>
            {
                var commandModules = _commandService.Modules
                    .Where(x => !x.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
                    .Select(x => ModuleHelpData.FromModuleInfo(x));

                var slashModules = _interactionService.Modules
                    .Where(x => !x.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
                    .Select(x => ModuleHelpData.FromModuleInfo(x));

                return commandModules.Concat(slashModules).ToArray();
            });

        /// <inheritdoc />
        public ModuleHelpData GetModuleHelpData(string query)
        {
            var allHelpData = GetModuleHelpData();

            var byNameExact = allHelpData.FirstOrDefault(x => x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (byNameExact != null)
                return byNameExact;

            var byTagsExact = allHelpData.FirstOrDefault(x => x.HelpTags.Any(y => y.Equals(query, StringComparison.OrdinalIgnoreCase)));
            if (byTagsExact != null)
                return byTagsExact;

            var byNameContains = allHelpData.FirstOrDefault(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (byNameContains != null)
                return byNameContains;

            var byTagsContains = allHelpData.FirstOrDefault(x => x.HelpTags.Any(y => y.Contains(query, StringComparison.OrdinalIgnoreCase)));
            if (byTagsContains != null)
                return byTagsContains;

            return null;
        }

        /// <inheritdoc />
        public CommandHelpData GetCommandHelpData(string query)
        {
            var allHelpData = GetModuleHelpData().SelectMany(x => x.Commands);

            var byModuleNameExact = allHelpData.FirstOrDefault(x => x.Aliases.Any(y => y.Equals(query, StringComparison.OrdinalIgnoreCase)));
            if (byModuleNameExact != null)
                return byModuleNameExact;

            var byNameContains =
                allHelpData.FirstOrDefault(x => x.Aliases.Any(y => y.Contains(query, StringComparison.OrdinalIgnoreCase)));
            if (byNameContains != null)
                return byNameContains;

            return null;
        }
    }
}
