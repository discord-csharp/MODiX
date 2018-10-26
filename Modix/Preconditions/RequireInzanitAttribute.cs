using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Preconditions
{
    public sealed class RequireInzanitAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is SocketGuildUser user)
            {
                PreconditionResult success;
                if (user.Id == 104975006542372864)
                    success = PreconditionResult.FromSuccess();
                else
                    success = PreconditionResult.FromError(
                        $"The value must be <@{104975006542372864}>");

                return Task.FromResult(success);
            }

            throw new ArgumentException($"{nameof(RequireInzanitAttribute)} can only be used on parameters of type ${typeof(SocketGuildUser).FullName}");
        }
    }
}