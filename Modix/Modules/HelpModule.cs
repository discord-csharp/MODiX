using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Modix.Services.CommandHelp;

namespace Modix.Modules
{
    [Name("Info"), Summary("General helper module")]
    public sealed class HelpModule : ModuleBase
    {
        private readonly CommandHelpService _commandService;

        public HelpModule(CommandHelpService cs)
        {
            _commandService = cs;
        }

        [Command("help"), Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync(string sendDm = null)
        {
            if (sendDm == null)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Help")
                    .WithDescription("Visit https://mod.gg/commands to view all the commands!")
                    .WithFooter("or do \"!help dm\" to have them DM'd to you (warning: spammy)");

                await ReplyAsync("", false, embed.Build());

                return;
            }

            var eb = new EmbedBuilder();
            var userDm = await Context.User.GetOrCreateDMChannelAsync();

            try
            {
                foreach (var module in _commandService.GetData())
                {
                    eb = eb.WithTitle($"Module: {module.Name ?? "Unknown"}")
                           .WithDescription(module.Summary ?? "Unknown");

                    foreach (var command in module.Commands)
                    {
                        eb.AddField(new EmbedFieldBuilder().WithName($"Command: !{command.Alias.ToLowerInvariant() ?? ""} {GetParams(command)}").WithValue(command.Summary ?? "Unknown"));
                    }

                    await userDm.SendMessageAsync(string.Empty, embed: eb.Build());
                    eb = new EmbedBuilder();
                }
            }
            catch (HttpException exc) when (exc.DiscordCode == 50007)
            {
                await ReplyAsync($"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so I can send you help.");
                return;
            }

            await ReplyAsync($"Check your private messages, {Context.User.Mention}");
        }

        private string GetParams(CommandHelpData info)
        {
            var sb = new StringBuilder();
            info.Parameters.ToList().ForEach(x =>
            {
                if (x.IsOptional)
                    sb.Append("[Optional(" + x.Name + ")]");
                else
                    sb.Append("[" + x.Name + "]");
            });
            return sb.ToString();
        }
    }
}
