using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using Modix.Services.AutoCodePaste;
using Modix.Services.FileUpload;
using Modix.Services.GuildInfo;

namespace Modix
{
    public class ModixBotHooks
    {
        private GuildInfoService _infoService;

        public ModixBotHooks(GuildInfoService infoService)
        {
            _infoService = infoService;
        }

        private readonly CodePasteHandler _codePaste = new CodePasteHandler(new CodePasteService());
        private readonly FileUploadHandler _fileUploadHandler = new FileUploadHandler();

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
            await _codePaste.ReactionAdded(message, channel, reaction);
        }

        public async Task HandleRemoveReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await _codePaste.ReactionRemoved(message, channel, reaction);
        }

        public Task HandleUserJoined(SocketGuildUser user)
        {
            _infoService.ClearCacheEntry(user.Guild);
            return Task.CompletedTask;
        }

        public Task HandleUserLeft(SocketGuildUser user)
        {
            _infoService.ClearCacheEntry(user.Guild);
            return Task.CompletedTask;
        }

        public async Task HandleMessage(SocketMessage messageParam)
        {
            var user = ((messageParam as SocketUserMessage)?.Author as SocketGuildUser);

            if (user == null) return;

            if (messageParam.Attachments.Any())
                await _fileUploadHandler.Handle(messageParam);
        }
    }
}
