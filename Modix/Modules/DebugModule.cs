using Discord;
using Discord.Commands;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Modix.Modules
{
    /// <summary>
    /// Used to test feature work on a private server. The contents of this module can be changed any time.
    /// </summary>
    [Group("debug"), RequireUserPermission(GuildPermission.Administrator)]
    public class DebugModule : ModuleBase
    {
        [Command("throw")]
        public Task Throw([Remainder]string text)
        {
            Log.Error("Error event");
            Log.Error(new Exception("ExceptionEvent"), "ExceptionEvent Template");
            Log.Information("Extra stuff we shouldn't see");

            return Task.CompletedTask;
        }
    }
}
