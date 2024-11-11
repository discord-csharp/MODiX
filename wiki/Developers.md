Below is a guide on how to start developing Modix. Depending on the nature of the features you want to add/work on, there's a few different workflows you might want to use, each of which have different requirements.

# Prerequisites
To work on Modix, you need a few things:
- A Discord application set up - [go here to create one](https://discord.com/developers/applications/), add a bot to it, and copy the **token** from the page. You can then add the bot to your server by going to ` https://discord.com/oauth2/authorize?scope=bot&permissions=0&client_id=[ID HERE]`, replacing `[ID HERE]` with the **Client ID** of your bot (not the token).
- Under `https://discord.com/developers/applications/{CLIENT ID HERE}/oauth` make sure to add redirect to your domain such as `https://localhost:5000/signin-discord` and make sure it is appended with `/signin-discord`
- [The latest .NET Core SDK for your chosen platform](https://www.microsoft.com/net/download) (currently 2.2)
- [NodeJS 10.x LTS for your chosen platform](https://nodejs.org/en/download/)
- [PostgreSQL database server](https://www.postgresql.org/download/). A docker container also works.
- **Optional**: [Docker](https://www.docker.com/get-docker). You **do not** need Docker if you're just developing locally - it's mostly just to test if your changes are significant enough that they might break CI, or if you prefer to keep your dev environment clean. If you're on Windows, make sure you switch to Linux containers.

If you're working on a feature that involves Modix's core services, and only needs to work with the Discord bot frontend, then you can proceed to opening `Modix.sln`.

# Setting Configuration
### Config file
You can specify configuration settings within **`developmentSettings.json`** when developing locally, or if copied into the Docker container, when developing with Docker. You can copy/paste **`developmentSettings.default.json`** as a template (make sure to remove the `.default` bit).
> **Note**: Be sure this is marked as "Copy If Newer" in your Solution Explorer / Properties.

### Environment Variables
If you prefer to use environment variables for configuration, they must all be prefixed with **`MODIX_`**. For example, **`MODIX_DbConnection`**, **`MODIX_DiscordToken`**, etc.

### List of config options
- **Required**
  - `DbConnection` - this is the connection string to your PostgreSQL instance. When developing locally, it'll probably be easiest to run Postgres in a container, or install it onto your development OS - whatever works. A simple connection string looks like: `Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;`
  - `DiscordToken` - this is the bot token Modix should use to connect to the Discord API. See above.
- **Recommended**
  - `DiscordClientId` - also taken from the Discord API page for your bot, this is needed for the web frontend's OAuth stuff to work properly. Can be ignored if you aren't working on/with the web frontend.
  - `DiscordClientSecret` - same as above.
- **Optional**
  - `MessageCacheSize` - An integer value defining the internal Discord.Net message cache size - used for logging deleted messages. Should be around 10 or more, and will default to that if unset, but you don't need it unless you're testing message deletion.
  - `LogWebhookId` - The ID of the Discord webhook to log to. Only necessary if you want log messages to appear in a channel on the server.
    - `https://discord.com/api/webhooks/[this part]/asda2ed2klkm5lkn42n34jk`
  - `LogWebhookToken` - Same as above, but the token of the webhook.
    - `https://discord.com/api/webhooks/000000000000000000/[this part]`
  - `ReplUrl` - The URL of the endpoint that will be receiving REPL (`!eval`/`!exec`) requests - required if you want to test the REPL, and requires you to host the [repl](https://github.com/discord-csharp/CSDiscord).
  - `IlUrl` - The URL of the endpoint that will be receiving IL (`!il`) requests. Same as above, and will likely be the same URL.

After setting all this up, you can move on to learning about [how modix starts](Modix-Startup)!
