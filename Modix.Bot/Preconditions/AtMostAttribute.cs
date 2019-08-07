using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Bot.Preconditions
{
    /// <summary>
    /// A parameter precondition which succeeds when the value passed
    /// is less than or equal to <see cref="MaxValue"/>.
    /// </summary>
    /// <example>
    /// [Command("reminder")]
    /// public async Task RemindAsync([AtMost(60)]int minutes, [Remainder]string message)
    /// {
    ///     await ReplyAsync("Ok! I'll remind you in {minutes} minute(s).");
    /// }
    /// </example>

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class AtMostAttribute : ParameterPreconditionAttribute
    {
        /// <summary>
        /// The maximum value that this precondition will accept, inclusively.
        /// </summary>
        public long MaxValue { get; }

        /// <summary>
        /// Creates a new instance of <see cref="AtMostAttribute"/>
        /// </summary>
        /// <param name="maximum">The maximum value that this precondition will accept, inclusively.</param>
        public AtMostAttribute(long maximum)
        {
            MaxValue = maximum;
        }

        /// <inheritdoc/>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            Preconditions.AllowedNumericType(parameter.Type, nameof(AtMostAttribute), nameof(value));

            if (NumericTypes.Comparators[parameter.Type](value, MaxValue) <= 0)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"Parameter {parameter.Name} must be less than {MaxValue}"));
        }
    }
}
