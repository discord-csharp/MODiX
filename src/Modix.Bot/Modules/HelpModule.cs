#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Discord.Net;

using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [ModuleHelp("Help", "Provides commands for helping users to understand how to interact with MODiX.")]
    public sealed class HelpModule : InteractionModuleBase
    {
        private readonly ICommandHelpService _commandHelpService;

        public HelpModule(ICommandHelpService commandHelpService)
        {
            _commandHelpService = commandHelpService;
        }

        [SlashCommand("help-dm", "Spams the user's DMs with a list of every command available.")]
        public async Task HelpDMAsync()
        {
            var userDM = await Context.User.CreateDMChannelAsync();

            foreach (var module in _commandHelpService.GetModuleHelpData().OrderBy(x => x.Name))
            {
                var embed = GetEmbedForModule(module);

                try
                {
                    await userDM.SendMessageAsync(embed: embed.Build());
                }
                catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
                {
                    await FollowupAsync(
                        $"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so that I can send you help.",
                        allowedMentions: new AllowedMentions { UserIds = new() { Context.User.Id } });
                    return;
                }
            }

            await FollowupAsync($"Check your private messages, {Context.User.Mention}.", allowedMentions: new AllowedMentions { UserIds = new() { Context.User.Id } });
        }

        [SlashCommand("help", "Retrieves help from a specific module or command.")]
        public async Task HelpAsync(
            [Summary(description: "Name of the module or command to query.")]
            string query)
        {
            await HelpAsync(query, HelpDataType.Command | HelpDataType.Module);
        }

        private async Task HelpAsync(string query, HelpDataType type)
        {
            if (TryGetEmbed(query, type, out var embed))
            {
                await FollowupAsync($"Results for \"{query}\":", embed: embed.Build(), allowedMentions: AllowedMentions.None);
                return;
            }

            await FollowupAsync($"Sorry, I couldn't find help related to \"{query}\".", allowedMentions: AllowedMentions.None);
        }

        private bool TryGetEmbed(string query, HelpDataType queries, [NotNullWhen(true)] out EmbedBuilder? embed)
        {
            embed = null;

            // Prioritize module over command.
            if (queries.HasFlag(HelpDataType.Module))
            {
                var byModule = _commandHelpService.GetModuleHelpData(query);
                if (byModule != null)
                {
                    embed = GetEmbedForModule(byModule);
                    return true;
                }
            }

            if (queries.HasFlag(HelpDataType.Command))
            {
                var byCommand = _commandHelpService.GetCommandHelpData(query);
                if (byCommand != null)
                {
                    embed = GetEmbedForCommand(byCommand);
                    return true;
                }
            }

            return false;
        }

        [Flags]
        public enum HelpDataType
        {
            Command = 1 << 1,
            Module = 1 << 2
        }

        private static EmbedBuilder GetEmbedForModule(ModuleHelpData module)
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

        private static EmbedBuilder GetEmbedForCommand(CommandHelpData command)
        {
            return AddCommandFields(new EmbedBuilder(), command);
        }

        private static EmbedBuilder AddCommandFields(EmbedBuilder embedBuilder, CommandHelpData command)
        {
            var summaryBuilder = new StringBuilder(command.Summary ?? "No summary.").AppendLine();
            var name = command.Aliases.FirstOrDefault() ?? command.Name;
            AppendAliases(summaryBuilder, command.Aliases.Where(a => !a.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList());
            AppendParameters(summaryBuilder, command.Parameters);

            var prefix = command.IsSlashCommand
                ? '/'
                : '!';

            embedBuilder.AddField(new EmbedFieldBuilder()
                                 .WithName($"Command: {prefix}{name} {GetParams(command)}")
                                 .WithValue(summaryBuilder.ToString()));

            return embedBuilder;
        }

        private static StringBuilder AppendAliases(StringBuilder stringBuilder, IReadOnlyCollection<string> aliases)
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

        private static StringBuilder AppendParameters(StringBuilder stringBuilder,
            IReadOnlyCollection<ParameterHelpData> parameters)
        {
            if (parameters.Count == 0)
                return stringBuilder;

            stringBuilder.AppendLine(Format.Bold("Parameters:"));

            foreach (var parameter in parameters)
            {
                if (parameter.Summary is not null)
                    stringBuilder.AppendLine($"• {Format.Bold(parameter.Name)}: {parameter.Summary}");
            }

            return stringBuilder;
        }

        private static string GetParams(CommandHelpData info)
        {
            var sb = new StringBuilder();

            foreach (var parameter in info.Parameters)
            {
                if (parameter.IsOptional)
                    sb.Append($"[{parameter.Name}]");
                else
                    sb.Append($"<{parameter.Name}>");
            }

            return sb.ToString();
        }
    }
}
