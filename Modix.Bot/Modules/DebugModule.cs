using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Modix.Bot.Extensions;
using Modix.Services.CommandHelp;

using Serilog;

namespace Modix.Modules
{
    /// <summary>
    /// Used to test feature work on a private server. The contents of this module can be changed any time.
    /// </summary>
    [Group("debug", "Used to test feature work on a private server. The contents of this module can be changed any time.")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    [HiddenFromHelp]
    public class DebugModule : InteractionModuleBase
    {
        [SlashCommand("throw", "Logs an exception.")]
        public async Task ThrowAsync()
        {
            Log.Error("Error event");
            Log.Error(new Exception("ExceptionEvent"), "ExceptionEvent Template");
            Log.Information("Extra stuff we shouldn't see");

            await Context.AddConfirmationAsync();
        }

        [SlashCommand("joined", "Displays all servers that the bot has joined.")]
        public async Task JoinedAsync()
        {
            var guilds = await Context.Client.GetGuildsAsync();

            var output = string.Join("\n", guilds.Select(a => $"{a.Id}: {a.Name}"));
            await FollowupAsync(output, allowedMentions: AllowedMentions.None);
        }
    }
}
