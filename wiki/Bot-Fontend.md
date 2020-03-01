# Bot Frontend

The Bot Frontend is the primary user interface of MODiX. It is the user interface that is accessed by users from within the Discord Web or Desktop application.

## Purpose

1. Provide simple, conveniently-accessible, user interfaces for MODiX functionality
2. Serve as the primary user interface for most day-to-day operations
3. Allow for command and rendering logic to be tested, independently from other layers of the application

## Methodology

### Commands

A command is a UI element that mimics a classical Command-Line Interface (CLI), consisting of an identifying prefix, a name, and any number of arguments, all provided through text. In the context of Discord.NET, commands are implemented through a listener, which checks every message received through the Discord Gateway connection for the presence of the identifying prefix (a `'!'` character, or other character set through config, or a mention of @MODiX itself). Messages that contain the identifying prefix are passed to Discord.NET's `CommandService` which parses the text to determine the name and arguments, and (if valid) invokes the corresponding Command method upon a Module class.

Commands are implemented in Discord.NET by subclassing `ModuleBase`, implementing methods to be invoked when the commands are executed, and annotating the class, the method, and its parameters with various Discord.NET attributes. These attributes serve to identify the command for the Discord.NET reflection system that maps available commands in memory to executable code, and define command metadata that is used by the Command Help system.

Generally, command methods should contain almost exclusively rendering code. Commands should generally invoke the [Business Logic](Business-Logic) layer to perform all the real work, by invoking service methods, or publishing request objects. Command results are also generally rendered into embeds.

Commands should always provide some kind of feedback to the user, especially when they do not produce any result data. This can be accomplished by attaching a reaction to the command message itself, indicating success or failure. The MODiX command system already implements rudimentary error handling and response for exceptions thrown by a command, in the form of an attached reaction that can be clicked to reveal a more detailed error message. Commands that may be long-running should also provide immediate confirmation that the command was received and is processing by attaching a reaction.

E.G.
```cs
[Group("infraction"), Alias("infractions")]
[Summary("Provides commands for working with infractions.")]
public class InfractionModule
    : ModuleBase
{
    ...
    [Command("delete")]
    [Summary("Marks an infraction as deleted, so it no longer appears within infraction search results.")]
    public async Task DeleteAsync(
        [Summary("The ID value of the infraction to be deleted.")]
            long infractionId)
    {
        await ModerationService.DeleteInfractionAsync(infractionId);
        await Context.AddConfirmation();
    }
    ...
}
```

### Logs

A log element is a Discord message sent by MODiX that is not directly triggered by an user command in that channel, containing renered data about some action that a user performed elsewhere, or in the Web Frontend, or that MODiX performed automatically, without user input. Like command results, logs are almost always generated in the form of an embed.

Logs are usually closely related to the Channel Designations system, which allows specific types of logs to be designated for sending to a specific channel or channels. 

E.G.
```cs
public class ModerationLoggingBehavior : IModerationActionEventHandler
{
    ...
    public async Task OnModerationActionCreatedAsync(long moderationActionId, ModerationActionCreationData data)
    {
        if (!await DesignatedChannelService.AnyDesignatedChannelAsync(data.GuildId, DesignatedChannelType.ModerationLog))
            return;

        var moderationAction = await ModerationService.GetModerationActionSummaryAsync(moderationActionId);

        if (!_renderTemplates.TryGetValue((moderationAction.Type, moderationAction.Infraction?.Type), out var renderTemplate))
            return;

        // De-linkify links in the message, otherwise Discord will make auto-embeds for them in the log channel
        var content = moderationAction.DeletedMessage?.Content.Replace("http://", "[redacted]").Replace("https://", "[redacted]");

        var message = ...

        await DesignatedChannelService.SendToDesignatedChannelsAsync(
            await DiscordClient.GetGuildAsync(data.GuildId), DesignatedChannelType.ModerationLog, message);
    }
    ...
}
```

### Dialogs

A dialog is mostly similar to a log element, except that the message itself acts as a mechanism for user input. This is accomplished by having MODiX track "dialog" messages long-term, and using reactions upon the message as makeshift buttons that the user can click. These inputs can be used to modify the message itself, to present new or different data, or to initiate some other MODiX operation.

### Authentication

Authentication is the task of determining, in a secure manner, what user is performing a particular user operation. For example, the authenticated user for a command is the user that authored the message, while the authenticated user for a dialog interaction is the user that added or removed a reaction from the dialog message. As these are user interactions specific to the Bot Frontend, Bot Frontend code should be responsible for performing authentication, by invoking `Modix.Services.Core.AuthenticationService.OnAuthenticatedAsync()`.

```cs
public class CommandListeningBehavior
    : INotificationHandler<MessageReceivedNotification>
{
    ...
    public async Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken = default)
    {
        ...

        if (!(userMessage.Author is IGuildUser author)
            || (author.Guild is null)
            || author.IsBot
            || author.IsWebhook)
            return;

        ...

        var commandContext = new CommandContext(DiscordClient, userMessage);

        await AuthorizationService.OnAuthenticatedAsync(author);

        ...
    }
    ...
}
```

### Testing

Testing should generally focus on behaviors and decision making. Where rendering is involved, testing should usually only worry about ensuring that rendering occurs without error, not necessarily examining render output. Where applicable, render output generated during testing can be dumped to the Console (which the testing framwork should intercept for logging purposes) where it can be visually examined by a developer.

See [Testing](Testing) for more information.
