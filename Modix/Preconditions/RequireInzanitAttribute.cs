using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Modix.Preconditions
{
    public sealed class RequireInzanitAttribute : ParameterPreconditionAttribute
    {
        private const ulong Inzanit = 104975006542372864;

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is SocketGuildUser user)
            {
                PreconditionResult success;
                if (user.Id == Inzanit)
                    success = PreconditionResult.FromSuccess();
                else
                    success = PreconditionResult.FromError(
                        $"The value must be <@{Inzanit}>");

                return Task.FromResult(success);
            }

            throw new ArgumentException($"{nameof(RequireInzanitAttribute)} can only be used on parameters of type ${typeof(SocketGuildUser).FullName}");
        }
    }
}