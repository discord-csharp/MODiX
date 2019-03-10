using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Humanizer;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;

namespace Modix.Modules
{
    [Name("Help")]
    [Summary("Provides commands for helping users to understand how to interact with MODiX.")]
    public sealed class HelpModule : ModuleBase
    {
        private readonly ICommandHelpService _commandHelpService;

        public HelpModule(ICommandHelpService commandHelpService)
        {
            _commandHelpService = commandHelpService;
        }

        [Command("help"), Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync()
        {
            var modules = _commandHelpService.GetModuleHelpData()
                .Select(d => d.Name)
                .OrderBy(d => d);

            var descriptionBuilder = new StringBuilder()
                .AppendLine("Modules:")
                .AppendJoin(", ", modules)
                .AppendLine()
                .AppendLine()
                .AppendLine("Do \"!help dm\" to have everything DMed to you. (Spammy!)")
                .AppendLine("Do \"!help [module name] to have that module's commands listed.")
                .AppendLine("Visit https://mod.gg/commands to view all the commands!");

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
        [Summary("Prints a neat list of all commands in the supplied module.")]
        [Priority(-10)]
        public async Task HelpAsync([Remainder]string moduleName)
        {
            var foundModule = _commandHelpService.GetModuleHelpData().FirstOrDefault(d => d.Name.IndexOf(moduleName, StringComparison.OrdinalIgnoreCase) >= 0);

            if (foundModule is null)
            {
                await ReplyAsync($"Sorry, I couldn't find the \"{moduleName}\" module.");
                return;
            }

            var embed = GetEmbedForModule(foundModule);

            await ReplyAsync($"Results for \"{moduleName}\":", embed: embed.Build());

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

            foreach (var alias in CollapsePlurals(aliases))
            {
                stringBuilder.AppendLine($"• {alias}");
            }

            return stringBuilder;
        }

        private IReadOnlyCollection<string> CollapsePlurals(IReadOnlyCollection<string> aliases)
        {
            var splitIntoWords = aliases.Select(x => x.Split(" ", StringSplitOptions.RemoveEmptyEntries));

            var withSingulars = splitIntoWords.Select(x =>
            (
                Singular: x.Select(y => y.Singularize(false)).ToArray(),
                Value: x
            ));

            var groupedBySingulars = withSingulars.GroupBy(x => x.Singular, x => x.Value, new SequenceEqualityComparer<string>());

            var withDistinctParts = new HashSet<string>[groupedBySingulars.Count()][];

            foreach (var (singular, singularIndex) in groupedBySingulars.AsIndexable())
            {
                var parts = new HashSet<string>[singular.Key.Count];

                for (var i = 0; i < parts.Length; i++)
                    parts[i] = new HashSet<string>();

                foreach (var variation in singular)
                {
                    foreach (var (part, partIndex) in variation.AsIndexable())
                    {
                        parts[partIndex].Add(part);
                    }
                }

                withDistinctParts[singularIndex] = parts;
            }

            var parenthesized = new string[withDistinctParts.Length][];

            foreach (var (alias, aliasIndex) in withDistinctParts.AsIndexable())
            {
                parenthesized[aliasIndex] = new string[alias.Length];

                foreach (var (word, wordIndex) in alias.AsIndexable())
                {
                    if (word.Count > 1)
                    {
                        var (longestForm, shortestForm) = word.First().Length > word.Last().Length
                            ? (word.First(), word.Last())
                            : (word.Last(), word.First());

                        var indexOfDifference = word.First()
                            .ZipOrDefault(word.Last())
                            .AsIndexable()
                            .First(x => x.Value.First != x.Value.Second)
                            .Index;

                        parenthesized[aliasIndex][wordIndex] = $"{longestForm.Substring(0, indexOfDifference)}({longestForm.Substring(indexOfDifference)})";
                    }
                    else
                    {
                        parenthesized[aliasIndex][wordIndex] = word.Single();
                    }
                }
            }

            var formatted = new string[parenthesized.Length];

            foreach (var (alias, aliasIndex) in parenthesized.AsIndexable())
            {
                formatted[aliasIndex] = string.Join(" ", alias);
            }

            return formatted;
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
