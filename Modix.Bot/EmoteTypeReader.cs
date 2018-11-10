﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Modix
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            if (Emote.TryParse(input, out Emote target))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(target));
            }
            else
            {
                var ret = new Emoji(input);

                if (ret.Name == null)
                {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not recognize emoji \"{ret}\""));
                }

                return Task.FromResult(TypeReaderResult.FromSuccess(ret));
            }
        }
    }
}
