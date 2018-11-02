using System;
using System.Collections.Generic;
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
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Help")
                .WithDescription
                (
                    "Modules:\n" +
                    string.Join(", ", _commandService.GetData().Select(d => d.Name)) + "\n\n" + 
                    "Do \"!help dm\" to have everything DM'd to you (spammy!)\n" +
                    "Do \"!help [module name] to have that module's commands listed\n" +
                    "Visit https://mod.gg/commands to view all the commands!"
                );

            await ReplyAsync("", false, embed.Build());
        }

        [Command("help"), Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync([Remainder]string moduleName)
        {
            var eb = new EmbedBuilder();
            var userDm = await Context.User.GetOrCreateDMChannelAsync();

            void AddCommandFields(IEnumerable<CommandHelpData> commands)
            {
                foreach (var command in commands)
                {
                    eb.AddField(new EmbedFieldBuilder().WithName($"Command: !{command.Alias.ToLowerInvariant() ?? ""} {GetParams(command)}").WithValue(command.Summary ?? "Unknown"));
                }
            }

            void BuildEmbedForModule(ModuleHelpData module)
            {
                eb = eb.WithTitle($"Module: {module.Name ?? "Unknown"}")
                           .WithDescription(module.Summary ?? "Unknown");

                AddCommandFields(module.Commands);
            }

            try
            {
                if (moduleName == "dm")
                {
                    foreach (var module in _commandService.GetData())
                    {
                        BuildEmbedForModule(module);

                        await userDm.SendMessageAsync(string.Empty, embed: eb.Build());
                        eb = new EmbedBuilder();
                    }

                    await ReplyAsync($"Check your private messages, {Context.User.Mention}");

                    return;
                }

                var foundModule = _commandService.GetData().FirstOrDefault(d => d.Name.IndexOf(moduleName, StringComparison.OrdinalIgnoreCase) >= 0);

                if (foundModule == null)
                {
                    await ReplyAsync($"Sorry, I couldn't find the \"{moduleName}\" module.");
                    return;
                }

                BuildEmbedForModule(foundModule);
                await ReplyAsync($"Results for \"{moduleName}\":", embed: eb.Build());
            }
            catch (HttpException exc) when (exc.DiscordCode == 50007)
            {
                await ReplyAsync($"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so I can send you help.");
            }
            
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