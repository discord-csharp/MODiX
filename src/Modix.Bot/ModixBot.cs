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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modix.Data.Models.Core;

namespace Modix
{
    public sealed class ModixBot : BackgroundService
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordRestClient _restClient;
        private readonly CommandService _commands;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _provider;
        private readonly ModixConfig _config;
        private readonly DiscordSerilogAdapter _serilogAdapter;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IHostEnvironment _env;
        private IServiceScope _scope;
        private readonly ConcurrentDictionary<ICommandContext, IServiceScope> _commandScopes = new();

        public ModixBot(
            DiscordSocketClient discordClient,
            DiscordRestClient restClient,
            IOptions<ModixConfig> modixConfig,
            CommandService commandService,
            InteractionService interactions,
            DiscordSerilogAdapter serilogAdapter,
            IHostApplicationLifetime applicationLifetime,
            IServiceProvider serviceProvider,
            ILogger<ModixBot> logger,
            IHostEnvironment env)
        {
            _client = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _config = modixConfig?.Value ?? throw new ArgumentNullException(nameof(modixConfig));
            _commands = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
            _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serilogAdapter = serilogAdapter ?? throw new ArgumentNullException(nameof(serilogAdapter));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
            Log = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env;
        }

        private ILogger<ModixBot> Log { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            Log.LogInformation("Starting bot background service.");

            IServiceScope scope = null;
            try
            {
                // Create a new scope for the session.
                scope = _provider.CreateScope();

                Log.LogTrace("Registering listeners for Discord client events.");

                _client.LatencyUpdated += OnLatencyUpdated;
                _client.Disconnected += OnDisconnect;

                _client.Log += _serilogAdapter.HandleLog;
                _restClient.Log += _serilogAdapter.HandleLog;
                _commands.Log += _serilogAdapter.HandleLog;

                // Register with the cancellation token so we can stop listening to client events if the service is
                // shutting down or being disposed.
                stoppingToken.Register(OnStopping);

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

                Log.LogInformation("Loading interaction modules...");

                var modules = (await _interactions.AddModulesAsync(typeof(ModixBot).Assembly, _scope.ServiceProvider)).ToArray();

                foreach (var guild in _client.Guilds)
                {
                    var commands = await _interactions.AddModulesToGuildAsync(guild, deleteMissing: true, modules);
                }

                Log.LogInformation("{Modules} interaction modules loaded.", modules.Length);
                Log.LogInformation("Loaded {SlashCommands} slash commands.", modules.SelectMany(x => x.SlashCommands).Count());
                Log.LogInformation("Loaded {ContextCommands} context commands.", modules.SelectMany(x => x.ContextCommands).Count());
                Log.LogInformation("Loaded {ModalCommands} modal commands.", modules.SelectMany(x => x.ModalCommands).Count());
                Log.LogInformation("Loaded {ComponentCommands} component commands.", modules.SelectMany(x => x.ComponentCommands).Count());

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
                _client.LatencyUpdated -= OnLatencyUpdated;

                _client.Log -= _serilogAdapter.HandleLog;
                _commands.Log -= _serilogAdapter.HandleLog;
                _restClient.Log -= _serilogAdapter.HandleLog;

                foreach (var context in _commandScopes.Keys)
                {
                    _commandScopes.TryRemove(context, out var commandScope);
                    commandScope?.Dispose();
                }
            }
        }

        private Task OnLatencyUpdated(int arg1, int arg2)
        {
            if (_env.IsProduction())
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
                Log.LogInformation("Received gateway reconnect");
                return Task.CompletedTask;
            }

            Log.LogInformation(ex, "The bot disconnected unexpectedly. Stopping the application.");
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
            var whenReadySource = new TaskCompletionSource<object>();

            try
            {
                _client.Ready += OnClientReady;

                cancellationToken.ThrowIfCancellationRequested();

                await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
                await _client.StartAsync();

                await _restClient.LoginAsync(TokenType.Bot, _config.DiscordToken);

                await whenReadySource.Task;
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
                await _client.SetGameAsync(_config.WebsiteBaseUrl);

                whenReadySource.SetResult(null);
            }
        }
    }
}
