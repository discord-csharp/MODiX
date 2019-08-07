using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Bot.Preconditions
{
    /// <summary>
    /// A parameter precondition which succeeds when the value passed
    /// is between <see cref="MinValue"/> and <see cref="MaxValue"/>,
    /// inclusively.
    /// </summary>
    /// <example>
    /// [Command("reminder")]
    /// public async Task RemindAsync([Between(5, 10)]int minutes, [Remainder]string message)
    /// {
    ///     await ReplyAsync($"Ok! I'll remind you in {minutes} minutes.");
    /// }
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class BetweenAttribute : ParameterPreconditionAttribute
    {
        private readonly AtLeastAttribute _atLeast;
        private readonly AtMostAttribute _atMost;

        /// <summary>
        /// The minimum value that this precondition will accept, inclusively.
        /// </summary>
        public long MinValue => _atLeast.MinValue;
        /// <summary>
        /// The maximum value that this precondition will accept, inclusively.
        /// </summary>
        public long MaxValue => _atMost.MaxValue;

        /// <summary>
        /// Creates a new instance of <see cref="BetweenAttribute"/>.
        /// </summary>
        /// <param name="minValue">The minimum value that this precondition will accept, inclusively.</param>
        /// <param name="maxValue">The maximum value that this precondition will accept, inclusively.</param>
        public BetweenAttribute(long minValue, long maxValue)
        {
            _atLeast = new AtLeastAttribute(minValue);
            _atMost = new AtMostAttribute(maxValue);
        }

        /// <inheritdoc/>
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            var atLeast = await _atLeast.CheckPermissionsAsync(context, parameter, value, services);
            if (!atLeast.IsSuccess)
                return atLeast;

            var atMost = await _atMost.CheckPermissionsAsync(context, parameter, value, services);
            if (!atMost.IsSuccess)
                return atMost;

            return PreconditionResult.FromSuccess();
        }
    }

}
