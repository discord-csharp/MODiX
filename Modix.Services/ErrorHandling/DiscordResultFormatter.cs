using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Modix.Common.ErrorHandling;
using Modix.Services.Core;

namespace Modix.Services.ErrorHandling
{
    /// <summary>
    /// Formats instances of <typeparamref name="TResult"/> into <see cref="EmbedBuilder"/>s for sending to Discord
    /// </summary>
    public abstract class DiscordResultFormatter<TResult> : IResultFormatter<TResult, EmbedBuilder> where TResult : ServiceResult
    {
        protected ICommandContext Context { get; private set; }

        public DiscordResultFormatter(ICommandContextAccessor contextAccessor)
        {
            Context = contextAccessor.Context;
        }

        /// <inheritdoc />
        public virtual EmbedBuilder Format(TResult result)
        {
            if (result.IsSuccess) { return null; }

            return new EmbedBuilder()
                .WithTitle("Uh oh, an error occurred.")
                .WithDescription(result.Error)
                .WithColor(new Color(255, 0, 0));
        }
    }

    //We need to seal the generic otherwise DI is not happy with us
    /// <inheritdoc />
    public class DefaultDiscordResultFormatter : DiscordResultFormatter<ServiceResult>
    {
        public DefaultDiscordResultFormatter(ICommandContextAccessor contextAccessor) : base(contextAccessor)
        {
        }
    }
}
