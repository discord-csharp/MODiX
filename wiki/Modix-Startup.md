Below is a rundown of `~/Modix.Bot/ModixBot.cs`, where startup occurs. This class, derived from `BackgroundService`, utilizes ASP.NET Core's ["Hosted Services"](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.2) implementation and is added to the service container at the bottom of `~/Modix/ServiceCollectionExtensions.cs`, after all the "core" Modix services - including our `DiscordSocketClient`.

Some services are grouped - you may see calls like
```csharp
services.AddModixCore()
    .AddModixModeration()
    .AddModixPromotions()
//etc
```

This is mostly for code clarity - you can see the specific services registered by these methods in `Modix.Services`. If you are adding a new service, you may find it best to add it to one of these groups, to the miscellaneous services within `Install()`, or in a new group entirely.

All of this registration happens before the actual "bot" starts - we leverage the aforementioned `BackgroundService` stuff to make sure all our dependencies are in order, and essentially run the bot as a service of the web application. By having a single shared ServiceProvider, we can ensure proper scoping and sharing of services that are shared across both Web and Discord frontends.

***

We start within the overridden `ExecuteAsync` method, which ASP.NET calls for us on startup. Here, we register event hooks for things like Discord.Net logging. We then do a manual initialization/migration of our database, to ensure that any pending migrations are applied right away (and errors are thrown before anything runs properly).
```csharp
scope.ServiceProvider.GetRequiredService<ModixContext>()
    .Database.Migrate();
```

We also start all of our internal `IBehavior` implementations, for things like automoderation and logging. If you're writing a feature that will make the most sense as a behavior, simply have it implement IBehavior and register it as explained above - the rest will be handled for you. See the existing implementations for examples.
```csharp
_hooks.ServiceProvider = _scope.ServiceProvider;

foreach (var behavior in _scope.ServiceProvider.GetServices<IBehavior>())
{
    await behavior.StartAsync();
    stoppingToken.Register(() => behavior.StopAsync().GetAwaiter().GetResult());
}
```

Speaking of commands, we also add all our command modules to the `CommandService` for Discord.Net's command stuff, and handle executing them within the `HandleCommand` method - each command gets its own scope within the context of the DI container, and if it errors, we use our slightly friendly event handler to inform the user. Your frontend code should prefer to provide friendly errors to the user, rather than let exceptions bubble up from the service layer for optimal UX.

Finally, we actually start the client & log in, then `await Task.Delay(-1)` to make sure we aren't killed prematurely. We also add a temporary hook to the Ready event, which is removed after the first time it's executed. This sets the bot's status once at startup, then removes itself so it doesn't fire again in the event of a reconnect.

```csharp
Log.LogTrace("Discord client is ready. Setting game status.");
_client.Ready -= OnClientReady;
await _client.SetGameAsync("https://mod.gg/");
```