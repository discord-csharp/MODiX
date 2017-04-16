using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Modix.Utilities
{
    public class LimitToChannelsAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var channelsConfig = Environment.GetEnvironmentVariable($"MODIX_LIMIT_MODULE_CHANNELS_{command.Module.Name.ToUpper()}");

            if (channelsConfig == null)
            {
                return Task.Run(() => PreconditionResult.FromSuccess());
            }

            var channels = channelsConfig.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return channels.Any(c => ulong.Parse(c) == context.Channel.Id)
                ? Task.Run(() => PreconditionResult.FromSuccess())
                : Task.Run(() => PreconditionResult.FromError("This command cannot run in this channel"));
        }
    }
}
