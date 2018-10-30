using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modix
{
    public class IEmoteTypeReader : TypeReader
    {
//TypeReaders have to be asynchronous but we don't do async work, so...
#pragma warning disable 1998
        public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            if (Emote.TryParse(input, out Emote target))
            {
                return TypeReaderResult.FromSuccess(target);
            }
            else
            {
                var ret = new Emoji(input);

                if (ret.Name == null)
                {
                    return TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not recognize emoji \"{ret}\"");
                }

                return TypeReaderResult.FromSuccess(ret);
            }
        }
#pragma warning restore 1998
    }
}
