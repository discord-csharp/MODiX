using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;

namespace Modix.Modules
{
    [Name("Moderation")]
    [Summary("Guild moderation commands")]
    public class ModerationModule : ModuleBase
    {
        public ModerationModule(IModerationService moderationService)
        {
            ModerationService = moderationService ?? throw new ArgumentNullException(nameof(moderationService));
        }

        public Task Warn()
            => throw new NotImplementedException();

        public Task RescindWarn()
            => throw new NotImplementedException();

        public Task Mute()
            => throw new NotImplementedException();

        public Task TempMute()
            => throw new NotImplementedException();

        public Task UnMute()
            => throw new NotImplementedException();

        public Task Ban()
            => throw new NotImplementedException();

        public Task UnBan()
            => throw new NotImplementedException();

        public Task Search()
            => throw new NotImplementedException();

        internal protected IModerationService ModerationService { get; }
    }
}
