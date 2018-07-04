using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.AutoCodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.FileUpload;
using Modix.Services.GuildInfo;
using Serilog;

namespace Modix
{
    public class ModixBotHooks
    {
        public IServiceProvider ServiceProvider { get; set; }

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

        public async Task HandleAddReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var codePaste = ServiceProvider.GetService(typeof(CodePasteHandler)) as CodePasteHandler;
            var errorHelper = ServiceProvider.GetService(typeof(CommandErrorHandler)) as CommandErrorHandler;

            await codePaste.ReactionAdded(message, channel, reaction);
            await errorHelper.ReactionAdded(message, channel, reaction);
        }

        public async Task HandleRemoveReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
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

        public async Task HandleMessage(SocketMessage messageParam)
        {
            var user = (messageParam as SocketUserMessage)?.Author as SocketGuildUser;

            if (user == null) return;

            var fileUploadHandler = ServiceProvider.GetService(typeof(FileUploadHandler)) as FileUploadHandler;

            if (messageParam.Attachments.Any()) await fileUploadHandler.Handle(messageParam);
        }
    }
}