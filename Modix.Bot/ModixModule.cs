using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Common.ErrorHandling;

namespace Modix
{
    public abstract class ModixModule : ModuleBase<SocketCommandContext>
    {
        private readonly IResultFormatManager _resultVisualizerFactory;

        public ModixModule(IResultFormatManager resultVisualizerFactory)
        {
            _resultVisualizerFactory = resultVisualizerFactory;
        }

        protected async Task HandleResultAsync<T>(T result) where T : ServiceResult
        {
            var embed = _resultVisualizerFactory.Format<T, EmbedBuilder>(result);
            await ReplyAsync("", embed: embed.Build());
        }

        protected async Task AddConfirmationOrHandleAsync(ServiceResult result)
        {
            if (result.IsSuccess)
            {
                await Context.AddConfirmation();
            }
            else
            {
                await HandleResultAsync(result);
            }
        }
    }
}
