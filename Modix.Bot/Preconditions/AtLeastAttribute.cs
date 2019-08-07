using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class AtLeastAttribute : ParameterPreconditionAttribute
    {
        /// <summary>
        /// A parameter precondition which succeeds when the value passed
        /// is greater than or equal to <see cref="MinValue"/>.
        /// </summary>
        /// <example>
        /// [Command("reminder")]
        /// public async Task RemindAsync([AtLeast(1)]int minutes, [Remainder]string message)
        /// {
        ///     await ReplyAsync("Ok! I'll remind you in {minutes} minute(s).");
        /// }
        /// </example>
        public long MinValue { get; }

        /// <summary>
        /// Specifies a minimum value for this parameter.
        /// </summary>
        /// <param name="minimum">The minimum value that this precondition will accept, inclusively.</param>
        public AtLeastAttribute(long minimum)
        {
            MinValue = minimum;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            Preconditions.AllowedNumericType(parameter.Type, nameof(AtLeastAttribute), nameof(value));

            if (NumericTypes.Comparators[parameter.Type](value, MinValue) >= 0)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"Parameter {parameter.Name} must be greater than or equal to {MinimumValue}"));
        }
    }
}
