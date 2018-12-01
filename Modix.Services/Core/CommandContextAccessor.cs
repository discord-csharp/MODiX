using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace Modix.Services.Core
{
    public interface ICommandContextAccessor
    {
        ICommandContext Context { get; set; }
    }

    public class CommandContextAccessor : ICommandContextAccessor
    {
        public ICommandContext Context { get; set; }
    }
}
