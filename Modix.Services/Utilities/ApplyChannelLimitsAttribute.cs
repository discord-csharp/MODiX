﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data;
using Modix.Data.Utilities;

namespace Modix.Services.Utilities
{
    public class ApplyChannelLimits : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider map)
        {
            using (var db = map.GetService<ModixContext>())
            {
                var limits = db.ChannelLimits
                    .Where(c =>
                        c.ModuleName.ToUpper() == command.Module.Name.ToUpper() &&
                        c.ChannelId == context.Channel.Id.ToLong());

                return limits != null && limits.Any()
                    ? Task.Run(() => PreconditionResult.FromSuccess())
                    : Task.Run(() => PreconditionResult.FromError($"This command cannot run in this channel."));
            }
        }
    }
}