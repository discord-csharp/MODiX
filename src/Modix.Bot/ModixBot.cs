using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modix.Bot.Notifications;
using Modix.Data.Models.Core;

namespace Modix.Bot
{
    public sealed class ModixBot(
        DiscordSocketClient discordSocketClient,
        DiscordRestClient discordRestClient,
        IOptions<ModixConfig> modixConfig,
        CommandService commandService,
        InteractionService interactionService,
        DiscordSerilogAdapter discordSerilogAdapter,
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        ILogger<ModixBot> logger,
        IHostEnvironment hostEnvironment) : BackgroundService
    {
        private IServiceScope _scope;
        private readonly ConcurrentDictionary<ICommandContext, IServiceScope> _commandScopes = new();
        private TaskCompletionSource<object> _whenReadySource;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            logger.LogInformation("Starting bot background service");

            IServiceScope scope = null;

            try
            {
                // Create a new scope for the session.
                scope = serviceProvider.CreateScope();

                logger.LogTrace("Registering listeners for Discord client events");

                discordSocketClient.LatencyUpdated += OnLatencyUpdated;
                discordSocketClient.Disconnected += OnDisconnect;
                discordSocketClient.Log += discordSerilogAdapter.HandleLog;
                discordSocketClient.Ready += OnClientReady;
                discordSocketClient.MessageReceived += OnMessageReceived;
                discordSocketClient.MessageUpdated += OnMessageUpdated;
                discordSocketClient.MessageDeleted += OnMessageDeleted;
                discordSocketClient.ReactionAdded += OnReactionAdded;
                discordSocketClient.ReactionRemoved += OnReactionRemoved;

                discordRestClient.Log += discordSerilogAdapter.HandleLog;
                commandService.Log += discordSerilogAdapter.HandleLog;

                stoppingToken.Register(OnStopping);

                // The only thing that could go wrong at this point is the client failing to login and start. Promote
                // our local service scope to a field so that it's available to the HandleCommand method once events
                // start firing after we've connected.
                _scope = scope;

                logger.LogInformation("Loading command modules...");

                await commandService.AddModulesAsync(typeof(ModixBot).Assembly, _scope.ServiceProvider);

                logger.LogInformation("{Modules} modules loaded, containing {Commands} commands",
                    commandService.Modules.Count(), commandService.Modules.SelectMany(d=>d.Commands).Count());

                logger.LogInformation("Logging into Discord and starting the client");

                await StartClient(stoppingToken);

                logger.LogInformation("Discord client started successfully");

                logger.LogInformation("Loading interaction modules...");

                var modules = (await interactionService.AddModulesAsync(typeof(ModixBot).Assembly, _scope.ServiceProvider)).ToArray();

                foreach (var guild in discordSocketClient.Guilds)
                {
                    var commands = await interactionService.AddModulesToGuildAsync(guild, deleteMissing: true, modules);
                }

                logger.LogInformation("{Modules} interaction modules loaded", modules.Length);
                logger.LogInformation("Loaded {SlashCommands} slash commands", modules.SelectMany(x => x.SlashCommands).Count());
                logger.LogInformation("Loaded {ContextCommands} context commands", modules.SelectMany(x => x.ContextCommands).Count());
                logger.LogInformation("Loaded {ModalCommands} modal commands", modules.SelectMany(x => x.ModalCommands).Count());
                logger.LogInformation("Loaded {ComponentCommands} component commands", modules.SelectMany(x => x.ComponentCommands).Count());

                await Task.Delay(-1, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while attempting to start the background service");

                try
                {
                    OnStopping();
                    logger.LogInformation("Logging out of Discord");
                    await discordSocketClient.LogoutAsync();
                }
                finally
                {
                    scope?.Dispose();
                    _scope = null;
                }

                throw;
            }

            return;

            void OnStopping()
            {
                logger.LogInformation("Stopping background service");

                UnregisterClientHandlers();

                commandService.Log -= discordSerilogAdapter.HandleLog;
                discordRestClient.Log -= discordSerilogAdapter.HandleLog;

                foreach (var context in _commandScopes.Keys)
                {
                    _commandScopes.TryRemove(context, out var commandScope);
                    commandScope?.Dispose();
                }
            }
        }

        private Task OnLatencyUpdated(int arg1, int arg2)
        {
            if (hostEnvironment.IsProduction())
            {
                return File.WriteAllTextAsync("healthcheck.txt", DateTimeOffset.UtcNow.ToString("o"));
            }

            return Task.CompletedTask;
        }

        private Task OnDisconnect(Exception ex)
        {
            // Reconnections are handled by Discord.NET, we
            // don't need to worry about handling this ourselves
            if(ex is GatewayReconnectException)
            {
                logger.LogInformation("Received gateway reconnect");
                return Task.CompletedTask;
            }

            logger.LogInformation(ex, "The bot disconnected unexpectedly. Stopping the application");
            hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        private async Task StartClient(CancellationToken cancellationToken)
        {
            _whenReadySource = new TaskCompletionSource<object>();

            try
            {

                cancellationToken.ThrowIfCancellationRequested();

                await discordSocketClient.LoginAsync(TokenType.Bot, modixConfig.Value.DiscordToken);
                await discordSocketClient.StartAsync();

                await discordRestClient.LoginAsync(TokenType.Bot, modixConfig.Value.DiscordToken);

                await _whenReadySource.Task;
            }
            catch (Exception)
            {
                UnregisterClientHandlers();
                throw;
            }
        }

        private void UnregisterClientHandlers()
        {
            discordSocketClient.LatencyUpdated -= OnLatencyUpdated;
            discordSocketClient.Disconnected -= OnDisconnect;
            discordSocketClient.Log -= discordSerilogAdapter.HandleLog;

            discordSocketClient.Ready -= OnClientReady;

            discordSocketClient.MessageReceived -= OnMessageReceived;
            discordSocketClient.MessageUpdated -= OnMessageUpdated;
            discordSocketClient.MessageDeleted -= OnMessageDeleted;
            discordSocketClient.ReactionAdded -= OnReactionAdded;
            discordSocketClient.ReactionRemoved -= OnReactionRemoved;
        }

        private async Task OnClientReady()
        {
            await discordSocketClient.SetGameAsync(modixConfig.Value.WebsiteBaseUrl);
            _whenReadySource.SetResult(null);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new MessageReceivedNotificationV3(arg));
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> cachedMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new MessageUpdatedNotificationV3(cachedMessage, newMessage, channel));
        }

        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new MessageDeletedNotificationV3(message, channel));
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new ReactionAddedNotificationV3(message, channel, reaction));
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new ReactionRemovedNotificationV3(message, channel, reaction));
        }

        public override void Dispose()
        {
            try
            {
                // If the service is currently running, this will cancel the cancellation token that was passed into
                // our ExecuteAsync method, unregistering our event handlers for us.
                base.Dispose();
            }
            finally
            {
                _scope?.Dispose();
                discordSocketClient.Dispose();
                discordRestClient.Dispose();
            }
        }
    }
}
