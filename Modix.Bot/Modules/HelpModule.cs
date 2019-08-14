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

        [Command("help")]
        [Summary("Prints a neat list of all commands.")]
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
                    await ReplyAsync(
                        $"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so that I can send you help.");
                    return;
                }
            }

            await ReplyAsync($"Check your private messages, {Context.User.Mention}.");
        }

        [Command("help")]
        [Summary("Retrieves help from a specific module or command.")]
        [Priority(-10)]
        public async Task HelpAsync(
            [Remainder] [Summary("Name of the module or command to query.")]
            string query)
        {
            await HelpAsync(query, HelpDataType.Command | HelpDataType.Module);
        }

        [Command("help module"), Alias("help modules")]
        [Summary("Retrieves help from a specific module. Useful for modules that have an overlapping command name.")]
        [Priority(-10)]
        public async Task HelpModuleAsync(
            [Remainder] [Summary("Name of the module to query.")]
            string query)
        {
            await HelpAsync(query, HelpDataType.Module);
        }

        [Command("help command"), Alias("help commands")]
        [Summary("Retrieves help from a specific command. Useful for commands that have an overlapping module name.")]
        [Priority(-10)]
        public async Task HelpCommandAsync(
            [Remainder] [Summary("Name of the module to query.")]
            string query)
        {
            await HelpAsync(query, HelpDataType.Command);
        }

        private async Task HelpAsync(string query, HelpDataType type)
        {
            var sanitizedQuery = FormatUtilities.SanitizeAllMentions(query);

            if (TryGetEmbed(query, type, out var embed))
                await ReplyAsync($"Results for \"{sanitizedQuery}\":", embed: embed.Build());

            await ReplyAsync($"Sorry, I couldn't find help related to \"{sanitizedQuery}\".");
        }

        private bool TryGetEmbed(string query, HelpDataType queries, out EmbedBuilder embed)
        {
            embed = null;

            if (queries.HasFlag(HelpDataType.Command))
            {
                var byCommand = _commandHelpService.GetCommandHelpData(query);
                if (byCommand != null)
                {
                    embed = GetEmbedForCommand(byCommand);
                }
            }

            if (queries.HasFlag(HelpDataType.Module) && embed is null)
            {
                var byModule = _commandHelpService.GetCommandHelpData(query);
                if (byModule != null)
                {
                    embed = GetEmbedForCommand(byModule);
                }
            }

            return embed != null;
        }

        [Flags]
        public enum HelpDataType
        {
            Command = 1 << 1,
            Module = 1 << 2
        }

        private EmbedBuilder GetEmbedForModule(ModuleHelpData module)
        {
            var embedBuilder = new EmbedBuilder()
                              .WithTitle($"Module: {module.Name}")
                              .WithDescription(module.Summary);

            foreach (var command in module.Commands)
            {
                AddCommandFields(embedBuilder, command);
            }

            return embedBuilder;
        }

        private EmbedBuilder GetEmbedForCommand(CommandHelpData command)
        {
            return AddCommandFields(new EmbedBuilder(), command);
        }

        private EmbedBuilder AddCommandFields(EmbedBuilder embedBuilder, CommandHelpData command)
        {
            var summaryBuilder = new StringBuilder(command.Summary ?? "No summary.").AppendLine();
            var name = command.Aliases.FirstOrDefault();
            AppendAliases(summaryBuilder, command.Aliases.Where(a => !a.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList());
            AppendParameters(summaryBuilder, command.Parameters);

            embedBuilder.AddField(new EmbedFieldBuilder()
                                 .WithName($"Command: !{name} {GetParams(command)}")
                                 .WithValue(summaryBuilder.ToString()));

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

        private StringBuilder AppendParameters(StringBuilder stringBuilder,
            IReadOnlyCollection<ParameterHelpData> parameters)
        {
            if (parameters.Count == 0)
                return stringBuilder;

            stringBuilder.AppendLine(Format.Bold("Parameters:"));

            foreach (var parameter in parameters)
            {
                if (!(parameter.Summary is null))
                    stringBuilder.AppendLine($"• {Format.Bold(parameter.Name)}: {parameter.Summary}");
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
