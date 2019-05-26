using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Help")]
    [Summary("Provides commands for helping users to understand how to interact with MODiX.")]
    public sealed class HelpModule : ModuleBase
    {
        private readonly ICommandHelpService _commandHelpService;
        private readonly ModixConfig _config;

        public HelpModule(ICommandHelpService commandHelpService, IOptions<ModixConfig> config)
        {
            _commandHelpService = commandHelpService;
            _config = config.Value;
        }

        [Command("help"), Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync()
        {
            var modules = _commandHelpService.GetModuleHelpData()
                .Select(d => d.Name)
                .OrderBy(d => d);

            // https://mod.gg/commands
            var url = new UriBuilder(_config.WebsiteBaseUrl)
            {
                Path = "/commands"
            }.RemoveDefaultPort().ToString();

            var descriptionBuilder = new StringBuilder()
                .AppendLine("Modules:")
                .AppendJoin(", ", modules)
                .AppendLine()
                .AppendLine()
                .AppendLine("Do \"!help dm\" to have everything DMed to you. (Spammy!)")
                .AppendLine("Do \"!help [module name] to have that module's commands listed.")
                .AppendLine($"Visit {url} to view all the commands!");

            var embed = new EmbedBuilder()
                .WithTitle("Help")
                .WithDescription(descriptionBuilder.ToString());

            await ReplyAsync(embed: embed.Build());
        }

        [Command("help dm")]
        [Summary("Spams the user's DMs with a list of every command available.")]
        public async Task HelpDMAsync()
        {
            var userDM = await Context.User.GetOrCreateDMChannelAsync();

            foreach (var module in _commandHelpService.GetModuleHelpData().OrderBy(x => x.Name))
            {
                var embed = GetEmbedForModule(module);

                try
                {
                    await userDM.SendMessageAsync(embed: embed.Build());
                }
                catch (HttpException ex) when (ex.DiscordCode == 50007)
                {
                    await ReplyAsync($"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so that I can send you help.");
                    return;
                }

            }

            await ReplyAsync($"Check your private messages, {Context.User.Mention}.");
        }

        [Command("help")]
        [Summary("Prints a neat list of all commands based on the supplied query.")]
        [Priority(-10)]
        public async Task HelpAsync(
            [Remainder]
            [Summary("The module name or related query to use to search for the help module.")]
                string query)
        {
            var foundModule = _commandHelpService.GetModuleHelpData(query);
            var sanitizedQuery = FormatUtilities.SanitizeAllMentions(query);

            if (foundModule is null)
            {
                await ReplyAsync($"Sorry, I couldn't find help related to \"{sanitizedQuery}\".");
                return;
            }

            var embed = GetEmbedForModule(foundModule);

            await ReplyAsync($"Results for \"{sanitizedQuery}\":", embed: embed.Build());

        }

        private EmbedBuilder GetEmbedForModule(ModuleHelpData module)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Module: {module.Name}")
                .WithDescription(module.Summary);

            return AddCommandFields(embedBuilder, module.Commands);
        }

        private EmbedBuilder AddCommandFields(EmbedBuilder embedBuilder, IEnumerable<CommandHelpData> commands)
        {
            foreach (var command in commands)
            {
                var summaryBuilder = new StringBuilder(command.Summary ?? "No summary.").AppendLine();
                var summary = AppendAliases(summaryBuilder, command.Aliases);

                embedBuilder.AddField(new EmbedFieldBuilder()
                    .WithName($"Command: !{command.Aliases.FirstOrDefault()} {GetParams(command)}")
                    .WithValue(summary.ToString()));
            }

            return embedBuilder;
        }

        private StringBuilder AppendAliases(StringBuilder stringBuilder, IReadOnlyCollection<string> aliases)
        {
            if (aliases.Count == 0)
                return stringBuilder;

            stringBuilder.AppendLine(Format.Bold("Aliases:"));

            foreach (var alias in FormatUtilities.CollapsePlurals(aliases))
            {
                stringBuilder.AppendLine($"• {alias}");
            }

            return stringBuilder;
        }

        private string GetParams(CommandHelpData info)
        {
            var sb = new StringBuilder();

            foreach (var parameter in info.Parameters)
            {
                if (parameter.IsOptional)
                    sb.Append($"[Optional({parameter.Name})]");
                else
                    sb.Append($"[{parameter.Name}]");
            }

            return sb.ToString();
        }
    }
}
