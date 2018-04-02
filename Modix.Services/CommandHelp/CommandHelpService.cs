using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;

namespace Modix.Services.CommandHelp
{
    public class CommandHelpService
    {
        private CommandService _commandService;
        private List<ModuleHelpData> _cachedHelpData;

        public CommandHelpService(CommandService commandService)
        {
            _commandService = commandService;
        }

        public List<ModuleHelpData> GetData()
        {
            if (_cachedHelpData == null)
            {
                _cachedHelpData = new List<ModuleHelpData>();

                foreach (var module in _commandService.Modules)
                {
                    if (module.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
                    {
                        continue;
                    }

                    _cachedHelpData.Add(ModuleHelpData.FromModuleInfo(module));
                }
            }

            return _cachedHelpData;
        }
    }
}
