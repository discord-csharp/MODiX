using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Modix
{
    public class EmoteTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(Type type)
            => typeof(IEmote).IsAssignableFrom(type);

        public override ApplicationCommandOptionType GetDiscordType()
            => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var input = option.Value.ToString();

            if (Emote.TryParse(input, out var target))
            {
                return Task.FromResult(TypeConverterResult.FromSuccess(target));
            }
            else if (Emoji.TryParse(input, out var emoji))
            {
                return Task.FromResult(TypeConverterResult.FromSuccess(emoji));
            }

            return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, $"Could not recognize emoji \"{input}\""));
        }
    }
}
