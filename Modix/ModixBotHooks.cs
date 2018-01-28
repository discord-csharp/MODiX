using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Data.Models;
using Serilog;
using Modix.Services.AutoCodePaste;
using Modix.Services.FileUpload;

namespace Modix
{
    public class ModixBotHooks
    {
        private readonly CodePasteHandler _codePaste = new CodePasteHandler();
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

        public async Task HandleMessage(SocketMessage message)
        {
            if (message.Attachments.Any())
                await _fileUploadHandler.Handle(message);
        }
    }
}
