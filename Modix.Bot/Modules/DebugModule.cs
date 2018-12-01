using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Serilog;

using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Modix.Services.CommandHelp;
using Modix.Common.ErrorHandling;
using Modix.Services.ErrorHandling;

namespace Modix.Modules
{
    /// <summary>
    /// Used to test feature work on a private server. The contents of this module can be changed any time.
    /// </summary>
    [Group("debug"), RequireUserPermission(GuildPermission.BanMembers), HiddenFromHelp]
    public class DebugModule : ModixModule
    {
        public DebugModule(IAuthorizationService authorizationService, IModerationService moderationService, IResultFormatManager resultVisualizerFactory)
            : base(resultVisualizerFactory)
        {
            AuthorizationService = authorizationService;
        }

        [Command("noclaim")]
        public async Task Claim()
        {
            var required = new AuthorizationClaim[]
            {
                AuthorizationClaim.PromotionsCloseCampaign,
                AuthorizationClaim.PromotionsComment,
                AuthorizationClaim.PromotionsCreateCampaign
            };

            var has = new AuthorizationClaim[]
            {
                AuthorizationClaim.PromotionsComment
            };

            await HandleResultAsync(new AuthResult(required, has));
        }

        [Command("throw")]
        public Task Throw([Remainder]string text)
        {
            Log.Error("Error event");
            Log.Error(new Exception("ExceptionEvent"), "ExceptionEvent Template");
            Log.Information("Extra stuff we shouldn't see");

            return Task.CompletedTask;
        }

        [Command("leave")]
        public async Task Leave(ulong guildId)
        {
            var guild = Context.Client.GetGuild(guildId);

            if(guild == null)
            {
                await ReplyAsync($"Modix is not joined to a guild with id {guildId}");
                return;
            }

            await guild.LeaveAsync();
            await ReplyAsync($"Left a guild named {guild.Name}");
        }

        [Command("joined")]
        public async Task Joined()
        {
            var guilds = Context.Client.Guilds;

            var output = string.Join(", ", guilds.Select(a => $"{a.Id}: {a.Name}"));
            await ReplyAsync(output);
        }

        internal protected IAuthorizationService AuthorizationService { get; }
    }
}
