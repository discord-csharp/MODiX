using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using Modix.Services.AutoCodePaste;
using Modix.Services.FileUpload;
using Modix.Services.GuildInfo;
using System;
using Modix.Handlers;
using Modix.Services.CommandHelp;
using Modix.Services.Configuration;

namespace Modix
{
    public class ModixBotHooks
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IConfigurationService ConfigurationService { get; set; }

        public Task HandleLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Log.Error(message.ToString());
                    break;
                case LogSeverity.Debug:
                    Log.Debug(message.ToString());
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.ToString());
                    break;
                case LogSeverity.Error:
                    Log.Error(message.ToString());
                    break;
                case LogSeverity.Info:
                    Log.Information(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message.ToString());
                    break;
            }
            return Task.CompletedTask;
        }

        public async Task HandleAddReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var codePaste = ServiceProvider.GetService(typeof(CodePasteHandler)) as CodePasteHandler;
            var errorHelper = ServiceProvider.GetService(typeof(CommandErrorHandler)) as CommandErrorHandler;

            await codePaste.ReactionAdded(message, channel, reaction);
            await errorHelper.ReactionAdded(message, channel, reaction);
        }

        public async Task HandleRemoveReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var codePaste = ServiceProvider.GetService(typeof(CodePasteHandler)) as CodePasteHandler;
            var errorHelper = ServiceProvider.GetService(typeof(CommandErrorHandler)) as CommandErrorHandler;

            await codePaste.ReactionRemoved(message, channel, reaction);
            await errorHelper.ReactionRemoved(message, channel, reaction);
        }

        public Task HandleUserJoined(SocketGuildUser user)
        {
            return InvalidateGuild(user.Guild);
        }

        public Task HandleUserLeft(SocketGuildUser user)
        {
            return InvalidateGuild(user.Guild);
        }

        private Task InvalidateGuild(IGuild guild)
        {
            var infoService = ServiceProvider.GetService(typeof(GuildInfoService)) as GuildInfoService;
            infoService.ClearCacheEntry(guild);

            return Task.CompletedTask;
        }

        public async Task HandleMessage(SocketMessage message)
        {
            var user = ((message as SocketUserMessage)?.Author as SocketGuildUser);

            if (user == null || user.IsBot) return;

            var inviteLinkHandler = (InviteLinkHandler)ServiceProvider.GetService(typeof(InviteLinkHandler));

            if(await inviteLinkHandler.PurgeInviteLink(message))
                return; // if we've purged the message with the invite link, return here

            if (message.Attachments.Any())
            {
                var fileUploadHandler = (FileUploadHandler)ServiceProvider.GetService(typeof(FileUploadHandler));

                await fileUploadHandler.Handle(message);
            }
        }
    }
}
