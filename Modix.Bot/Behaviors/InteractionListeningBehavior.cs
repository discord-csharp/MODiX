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

            await _authorizationService.OnAuthenticatedAsync(author.Id, author.Guild.Id, author.RoleIds.ToArray());

            string interactionName;
            bool isDeferred = false;

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

                    if (commandInfo is not null)
                    {
                        if (!commandInfo.Attributes.Any(x => x is DoNotDeferAttribute))
                        {
                            isDeferred = true;
                            await command.DeferAsync(ephemeral: command is not ISlashCommandInteraction);
                        }
                    }
                    else
                    {
                        Log.Error("Could not find command {Name}.", command.CommandName);
                        isDeferred = true;
                        await command.DeferAsync();
                    }

                    interactionName = commandInfo?.ToString() ?? command.CommandName;
                    break;
                case SocketModal modal:
                    interactionName = modal.Data.CustomId;
                    break;
                default:
                    interactionName = interaction.Type.ToString();
                    break;
            }

            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
            {
                var errorMessage = $"Error: {result.ErrorReason}";

                if (isDeferred)
                {
                    await context.Interaction.FollowupAsync(errorMessage, allowedMentions: AllowedMentions.None);
                }
                else
                {
                    await context.Interaction.RespondAsync(errorMessage, allowedMentions: AllowedMentions.None);
                }
            }

            stopwatch.Stop();
            Log.Information("Interaction took {Duration}ms to process: {Name}", stopwatch.ElapsedMilliseconds, interactionName);
        }
    }
}
