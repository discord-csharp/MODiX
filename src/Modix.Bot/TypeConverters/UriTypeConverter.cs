using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Modix.Bot.TypeConverters
{
    public class UriTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(Type type)
            => typeof(Uri).IsAssignableFrom(type);

        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            if (Uri.TryCreate(option.Value.ToString(), UriKind.Absolute, out var uri))
                return Task.FromResult(TypeConverterResult.FromSuccess(uri));

            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Failed to parse URI."));
        }
    }
}
