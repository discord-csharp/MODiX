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
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.BehaviourConfiguration;
using Modix.Services.CommandHelp;

namespace Modix
{
    public class ModixBotHooks
    {
        public IServiceProvider ServiceProvider { get; set; }
        public ulong CurrentBotId { get; set; }

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
            using (var scope = ServiceProvider.CreateScope())
            {
                var codePaste = scope.ServiceProvider.GetRequiredService<CodePasteHandler>();
                var errorHelper = scope.ServiceProvider.GetRequiredService<CommandErrorHandler>();

                await codePaste.ReactionAdded(message, channel, reaction);
                await errorHelper.ReactionAdded(message, channel, reaction);
            }
        }

        public async Task HandleRemoveReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var codePaste = scope.ServiceProvider.GetRequiredService<CodePasteHandler>();
                var errorHelper = scope.ServiceProvider.GetRequiredService<CommandErrorHandler>();

                await codePaste.ReactionRemoved(message, channel, reaction);
                await errorHelper.ReactionRemoved(message, channel, reaction);
            }
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
            using (var scope = ServiceProvider.CreateScope())
            {
                var infoService = scope.ServiceProvider.GetRequiredService<GuildInfoService>();
                infoService.ClearCacheEntry(guild);

                return Task.CompletedTask;
            }
        }

        public async Task HandleMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (message.Value?.Author.Id == CurrentBotId) { return; }

            using (var scope = ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<MessageLogHandler>()
                    .LogMessageDelete(message, channel);
            }
        }

        public async Task HandleMessageEdit(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel channel)
        {
            if (updatedMessage.Author.Id == CurrentBotId) { return; }

            using (var scope = ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<MessageLogHandler>()
                    .LogMessageEdit(await originalMessage.GetOrDownloadAsync(), updatedMessage, channel);
            }
        }

        public async Task HandleMessage(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) || !(userMessage.Author is SocketGuildUser || userMessage.Author.IsBot))
                return;

            using (var scope = ServiceProvider.CreateScope())
            {
                var inviteLinkHandler = scope.ServiceProvider.GetRequiredService<InviteLinkHandler>();

                if (await inviteLinkHandler.PurgeInviteLink(message))
                    return;

                if (message.Attachments.Any())
                {
                    var fileUploadHandler = scope.ServiceProvider.GetRequiredService<FileUploadHandler>();

                    await fileUploadHandler.Handle(message);
                }
            }
        }
    }
}
