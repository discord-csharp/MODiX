using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services;
using Modix.Services.BehaviourConfiguration;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix
{
    public sealed class ModixBot : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordRestClient _restClient;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        private readonly ModixConfig _config;
        private readonly DiscordSerilogAdapter _serilogAdapter;
        private readonly IApplicationLifetime _applicationLifetime;
        private IServiceScope _scope;

        public ModixBot(
            DiscordSocketClient discordClient,
            DiscordRestClient restClient,
            ModixConfig modixConfig,
            CommandService commandService,
            DiscordSerilogAdapter serilogAdapter,
            IApplicationLifetime applicationLifetime,
            IServiceProvider serviceProvider,
            ILogger<ModixBot> logger)
        {
            _client = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _config = modixConfig ?? throw new ArgumentNullException(nameof(modixConfig));
            _commands = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serilogAdapter = serilogAdapter ?? throw new ArgumentNullException(nameof(serilogAdapter));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            Log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private ILogger<ModixBot> Log { get; }
        private DiscordSerilogAdapter SerilogAdapter { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.LogInformation("Starting bot background service.");

            IServiceScope scope = null;
            try
            {
                // Create a new scope for the session.
                scope = _provider.CreateScope();

                Log.LogTrace("Registering listeners for Discord client events.");

                _client.Disconnected += OnDisconnect;
                _client.MessageReceived += HandleCommand;

                _client.Log += _serilogAdapter.HandleLog;
                _restClient.Log += _serilogAdapter.HandleLog;
                _commands.Log += _serilogAdapter.HandleLog;

                // Register with the cancellation token so we can stop listening to client events if the service is
                // shutting down or being disposed.
                stoppingToken.Register(OnStopping);

                Log.LogInformation("Running database migrations.");
                scope.ServiceProvider.GetRequiredService<ModixContext>()
                    .Database.Migrate();

                Log.LogInformation("Starting behaviors.");
                await scope.ServiceProvider.GetRequiredService<IBehaviourConfigurationService>()
                    .LoadBehaviourConfiguration();

                foreach (var behavior in scope.ServiceProvider.GetServices<IBehavior>())
                {
                    await behavior.StartAsync();
                    stoppingToken.Register(() => behavior.StopAsync().GetAwaiter().GetResult());
                }

                // The only thing that could go wrong at this point is the client failing to login and start. Promote
                // our local service scope to a field so that it's available to the HandleCommand method once events
                // start firing after we've connected.
                _scope = scope;

                Log.LogInformation("Loading command modules...");

                await _commands.AddModulesAsync(typeof(ModixBot).Assembly, _scope.ServiceProvider);

                Log.LogInformation("{Modules} modules loaded, containing {Commands} commands",
                    _commands.Modules.Count(), _commands.Modules.SelectMany(d=>d.Commands).Count());

                Log.LogInformation("Logging into Discord and starting the client.");

                await StartClient(stoppingToken);

                Log.LogInformation("Discord client started successfully.");

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "An error occurred while attempting to start the background service.");

                try
                {
                    OnStopping();

                    Log.LogInformation("Logging out of Discord.");
                    await _client.LogoutAsync();
                }
                finally
                {
                    scope?.Dispose();
                    _scope = null;
                }

                throw;
            }

            void OnStopping()
            {
                Log.LogInformation("Stopping background service.");

                _client.Disconnected -= OnDisconnect;
                _client.MessageReceived -= HandleCommand;

                _client.Log -= _serilogAdapter.HandleLog;
                _commands.Log -= _serilogAdapter.HandleLog;

                _restClient.Log -= _serilogAdapter.HandleLog;
            }
        }

        private Task OnDisconnect(Exception ex)
        {
            Log.LogInformation(ex, "The bot has disconnected unexpectedly. Stopping the application.");
            _applicationLifetime.StopApplication();
            return Task.CompletedTask;
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
                _client.Dispose();
                _restClient.Dispose();
            }
        }

        private async Task StartClient(CancellationToken cancellationToken)
        {
            try
            {
                _client.Ready += OnClientReady;

                cancellationToken.ThrowIfCancellationRequested();

                await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
                await _client.StartAsync();

                await _restClient.LoginAsync(TokenType.Bot, _config.DiscordToken);
            }
            catch (Exception)
            {
                _client.Ready -= OnClientReady;

                throw;
            }

            async Task OnClientReady()
            {
                Log.LogTrace("Discord client is ready. Setting game status.");
                _client.Ready -= OnClientReady;
                await _client.SetGameAsync("https://mod.gg/");
            }
        }

        private async Task HandleCommand(SocketMessage messageParam)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!(messageParam is SocketUserMessage message))
                return;

            if (!(message.Author is IGuildUser guildUser)
                || guildUser.IsBot
                || guildUser.IsWebhook)
                return;

            var argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            if (message.Content.Length <= 1)
                return;

            var context = new CommandContext(_client, message);

            using (var scope = _scope.ServiceProvider.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<IAuthorizationService>()
                    .OnAuthenticatedAsync(context.User as IGuildUser);

                var result = await _commands.ExecuteAsync(context, argPos, scope.ServiceProvider);

                if (!result.IsSuccess)
                {
                    var error = $"{result.Error}: {result.ErrorReason}";

                    if (!string.Equals(result.ErrorReason, "UnknownCommand", StringComparison.OrdinalIgnoreCase))
                    {
                        Log.LogWarning(error);
                    }
                    else
                    {
                        Log.LogError(error);
                    }

                    if (result.Error != CommandError.Exception)
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<CommandErrorHandler>();
                        await handler.AssociateError(message, error);
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Error: " + error);
                    }
                }
            }

            stopwatch.Stop();
            Log.LogInformation($"Took {stopwatch.ElapsedMilliseconds}ms to process: {message}");
        }
    }
}
