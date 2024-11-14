using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Bot.Responders.CommandErrors;
using Modix.Services;
using Modix.Services.Core;
using Serilog;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Modix.Bot.Responders;

public class CommandResponder(
    ICommandPrefixParser commandPrefixParser,
    IServiceProvider serviceProvider,
    CommandService commandService,
    CommandErrorService commandErrorService,
    IDiscordClient discordClient,
    IAuthorizationService authorizationService,
    IScopedSession scopedSession) : INotificationHandler<MessageReceivedNotificationV3>
{
    public async Task Handle(MessageReceivedNotificationV3 notification, CancellationToken cancellationToken = default)
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

        var argPos = await commandPrefixParser.TryFindCommandArgPosAsync(userMessage, cancellationToken);

        if (argPos is null)
            return;

        var commandContext = new CommandContext(discordClient, userMessage);

        var discordSession = (DiscordBotSession)scopedSession;
        discordSession.ApplyCommandContext(commandContext);

        await authorizationService.OnAuthenticatedAsync(author.Id, author.Guild.Id, author.RoleIds.ToList());

        var commandResult =  await commandService.ExecuteAsync(commandContext, argPos.Value, serviceProvider);

        if(!commandResult.IsSuccess)
        {
            var error = $"{commandResult.Error}: {commandResult.ErrorReason}";

            if (string.Equals(commandResult.ErrorReason, "UnknownCommand", StringComparison.OrdinalIgnoreCase))
            {
                Log.Error(error);
            }
            else
            {
                Log.Warning(error);
            }

            if (commandResult.Error == CommandError.Exception)
            {
                await commandContext.Channel.SendMessageAsync($"Error: {commandResult.ErrorReason}", allowedMentions: AllowedMentions.None);
            }
            else
            {
                await commandErrorService.SignalError(userMessage, error);
            }
        }

        stopwatch.Stop();

        Log.Information("Command took {StopwatchElapsedMilliseconds}ms to process: {CommandContextMessage}",
            stopwatch.ElapsedMilliseconds,
            commandContext.Message);
    }
}
