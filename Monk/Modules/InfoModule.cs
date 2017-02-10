using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace Monk.Modules
{
    [Name("Info"), Summary("General helper module")]
    public class InfoModule : ModuleBase
    {
        private CommandService commandService;

        public InfoModule(CommandService cs)
        {
            commandService = cs;
        }

        [Command("help"), Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync()
        {
            var eb = new EmbedBuilder();
            
            foreach(var module in commandService.Modules)
            {
                eb = eb.WithTitle(module.Name)
                       .WithDescription(module.Summary);

                foreach(var command in module.Commands)
                {
                    eb.AddField(new EmbedFieldBuilder().WithName(command.Name).WithValue(command.Summary));
                }
                await ReplyAsync(string.Empty, embed: eb.Build());
                eb = new EmbedBuilder();
            }
        }
    }
}
