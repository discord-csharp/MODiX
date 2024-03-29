using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Common.Messaging;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Utilities;

using Serilog;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Modix.Bot.Behaviors
{
    /// <summary>
    /// Listens for user commands within messages received from Discord, and executes them, if found.
    /// </summary>
    public class CommandListeningBehavior : INotificationHandler<MessageReceivedNotification>
    {
        /// <summary>
        /// Constructs a new <see cref="CommandListeningBehavior"/>, with the given dependencies.
        /// </summary>
        public CommandListeningBehavior(
            ICommandPrefixParser commandPrefixParser,
            IServiceProvider serviceProvider,
            CommandService commandService,
            CommandErrorHandler commandErrorHandler,
            IDiscordClient discordClient,
            IAuthorizationService authorizationService)
        {
            _commandPrefixParser = commandPrefixParser;
            ServiceProvider = serviceProvider;
            CommandService = commandService;
            CommandErrorHandler = commandErrorHandler;
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken = default)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!(notification.Message is IUserMessage userMessage)
                || (userMessage.Author is null))
                return;

            if (!(userMessage.Author is IGuildUser author)
                || (author.Guild is null)
                || author.IsBot
                || author.IsWebhook)
                return;

            if (userMessage.Content.Length <= 1)
                return;

            var argPos = await _commandPrefixParser.TryFindCommandArgPosAsync(userMessage, cancellationToken);
            if (argPos is null)
                return;

            var commandContext = new CommandContext(DiscordClient, userMessage);

            await AuthorizationService.OnAuthenticatedAsync(author.Id, author.Guild.Id, author.RoleIds.ToList());

            var commandResult =  await CommandService.ExecuteAsync(commandContext, argPos.Value, ServiceProvider);

            if(!commandResult.IsSuccess)
            {
                var error = $"{commandResult.Error}: {commandResult.ErrorReason}";

                if (string.Equals(commandResult.ErrorReason, "UnknownCommand", StringComparison.OrdinalIgnoreCase))
                    Log.Error(error);
                else
                    Log.Warning(error);

                if (commandResult.Error == CommandError.Exception)
                    await commandContext.Channel.SendMessageAsync($"Error: {commandResult.ErrorReason}", allowedMentions: AllowedMentions.None);
                else
                    await CommandErrorHandler.AssociateErrorAsync(userMessage, error);
            }

            stopwatch.Stop();
            Log.Information($"Command took {stopwatch.ElapsedMilliseconds}ms to process: {commandContext.Message}");
        }

        /// <summary>
        /// The <see cref="IServiceProvider"/> for the current service scope.
        /// </summary>
        internal protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// A <see cref="Discord.Commands.CommandService"/> used to parse and execute commands.
        /// </summary>
        internal protected CommandService CommandService { get; }

        /// <summary>
        /// A <see cref="Services.CommandHelp.CommandErrorHandler"/> used to report and track command errors, in the Discord UI.
        /// </summary>
        internal protected CommandErrorHandler CommandErrorHandler { get; }

        /// <summary>
        /// An <see cref="IDiscordClient"/> used to interact with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> used to interact with the application authorization system.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        private readonly ICommandPrefixParser _commandPrefixParser;
   }
}
