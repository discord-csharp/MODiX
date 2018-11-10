using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.AutoCodePaste;
using Modix.Services.GuildInfo;
using System;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.CommandHelp;
using Microsoft.Extensions.Logging;

namespace Modix
{
    public class ModixBotHooks
    {
        private readonly ILogger<ModixBotHooks> _log;

        public ModixBotHooks(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _log = ServiceProvider.GetRequiredService<ILogger<ModixBotHooks>>();
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public Task HandleLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _log.LogError(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Debug:
                    _log.LogDebug(message.ToString());
                    break;
                case LogSeverity.Warning:
                    _log.LogWarning(message.ToString());
                    break;
                case LogSeverity.Error:
                    _log.LogError(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Info:
                    _log.LogInformation(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    _log.LogTrace(message.ToString());
                    break;
            }
            return Task.CompletedTask;
        }

        public async Task HandleAddReaction(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                //var codePaste = scope.ServiceProvider.GetRequiredService<CodePasteHandler>();
                var errorHelper = scope.ServiceProvider.GetRequiredService<CommandErrorHandler>();

                //await codePaste.ReactionAdded(message, channel, reaction);
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
    }
}
