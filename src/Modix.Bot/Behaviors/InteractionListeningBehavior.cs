#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Modix.Bot.Attributes;
using Modix.Common.Messaging;
using Modix.Services.Core;

using Serilog;

namespace Modix.Bot.Behaviors
{
    public class InteractionListeningBehavior : INotificationHandler<InteractionCreatedNotification>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly InteractionService _interactionService;
        private readonly DiscordSocketClient _client;
        private readonly IAuthorizationService _authorizationService;

        public InteractionListeningBehavior(IServiceProvider serviceProvider, InteractionService interactionService, DiscordSocketClient client,
            IAuthorizationService authorizationService)
        {
            _serviceProvider = serviceProvider;
            _interactionService = interactionService;
            _client = client;
            _authorizationService = authorizationService;
        }

        public async Task HandleNotificationAsync(InteractionCreatedNotification notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var context = new SocketInteractionContext(_client, notification.Interaction);
            await ExecuteInteractionAsync(notification.Interaction, context);
        }

        private async Task ExecuteInteractionAsync(SocketInteraction interaction, IInteractionContext context)
        {
            if (interaction.User is not IGuildUser author || author.IsBot || author.IsWebhook)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            string interactionName;
            var defer = false;
            var requiresAuthentication = true;
            var ephemeralErrors = false;

            switch (interaction)
            {
                case SocketCommandBase command:
                    ICommandInfo? commandInfo = command switch
                    {
                        ISlashCommandInteraction slashCommand => _interactionService.SearchSlashCommand(slashCommand).Command,
                        IUserCommandInteraction userCommand => _interactionService.SearchUserCommand(userCommand).Command,
                        IMessageCommandInteraction messageCommand => _interactionService.SearchMessageCommand(messageCommand).Command,
                        _ => null,
                    };

                    interactionName = commandInfo?.ToString() ?? command.CommandName;
                    (defer, ephemeralErrors) = GetCommandInfoData(commandInfo, interactionName);

                    if (defer)
                        await command.DeferAsync(ephemeral: command is not ISlashCommandInteraction);
                    break;
                case SocketModal modal:
                    interactionName = modal.Data.CustomId;
                    var modalBaseCustomId = GetBaseCustomId(modal.Data.CustomId);
                    var modalCommandInfo = _interactionService.ModalCommands.FirstOrDefault(x => GetBaseCustomId(x.Name) == modalBaseCustomId);
                    (defer, ephemeralErrors) = GetCommandInfoData(modalCommandInfo, interactionName);

                    if (defer)
                        await modal.DeferAsync();
                    break;
                case SocketAutocompleteInteraction autocomplete:
                    requiresAuthentication = false;
                    interactionName = $"Autocomplete for {autocomplete.Data.CommandName} command";
                    break;
                case SocketMessageComponent messageComponent:
                    interactionName = messageComponent.Data.CustomId;
                    var messageComponentInfo = _interactionService.SearchComponentCommand(messageComponent).Command;
                    (defer, ephemeralErrors) = GetCommandInfoData(messageComponentInfo, interactionName);

                    if (defer)
                        await messageComponent.DeferAsync();
                    break;
                default:
                    interactionName = interaction.Type.ToString();
                    break;
            }

            if (requiresAuthentication)
                await _authorizationService.OnAuthenticatedAsync(author.Id, author.Guild.Id, author.RoleIds.ToArray());

            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
            {
                var errorMessage = $"Error: {result.ErrorReason}";

                if (defer)
                {
                    await context.Interaction.FollowupAsync(errorMessage, ephemeral: ephemeralErrors, allowedMentions: AllowedMentions.None);
                }
                else
                {
                    await context.Interaction.RespondAsync(errorMessage, ephemeral: ephemeralErrors, allowedMentions: AllowedMentions.None);
                }
            }

            stopwatch.Stop();
            Log.Information("Interaction took {Duration}ms to process: {Name}", stopwatch.ElapsedMilliseconds, interactionName);

            static (bool defer, bool ephemeralErrors) GetCommandInfoData(ICommandInfo? commandInfo, string commandName)
            {
                var (defer, ephemeralErrors) = (false, commandInfo is ComponentCommandInfo);

                if (commandInfo is not null)
                {
                    if (commandInfo.Attributes.Any(x => x is EphemeralErrorsAttribute))
                        ephemeralErrors = true;

                    if (!commandInfo.Attributes.Any(x => x is DoNotDeferAttribute))
                        defer = true;
                }
                else
                {
                    Log.Error("Could not find command {Name}.", commandName);
                    defer = true;
                }

                return (defer, ephemeralErrors);
            }

            static string GetBaseCustomId(string customId)
            {
                var colonIndex = customId.IndexOf(':');

                return colonIndex > -1
                    ? customId[..colonIndex]
                    : customId;
            }
        }
    }
}
