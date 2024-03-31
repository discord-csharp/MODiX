# MODiX - .NET C# Discord Bot

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/discord-csharp/MODiX/build-container.yml)
![Discord](https://img.shields.io/discord/143867839282020352)
![GitHub License](https://img.shields.io/github/license/discord-csharp/MODiX)

MODiX is a .NET C# Discord Bot that focuses on versatility, moderation and fun tools, primarily for users of the the largest [C# Discord](https://discord.gg/csharp) guild. MODiX's featureset is driven primarily by the C# guilds' needs, but feature requests, suggestions and other contributions are welcome and the bot is fully self hostable, and multi-tenant capable.

MODiX's primary features:
- C# REPL, to execute C# directly in Discord
- Persistent user notes, warnings, muting and banning
- Message quoting embeds
- Custom inline message tags, acting as macros to send templated messages that frequently get mentioned

If you want to report issues, discuss development or simply meet those who maintain MODiX, jump in on the dedicated [MODiX-development channel on Discord](https://discord.com/channels/143867839282020352/536023005164470303).

## How to contribute

### Development environment

You will need the .NET 8 SDK installed. MODiX is developed in VS Code, Visual Studio and Rider to maximise developer satisfaction.

We recommend you set up a containerised environment. MODiX is powered by a PostgreSQL database, and the easiest way to get started is by using Docker (or equivalent). Steps below apply if you use Docker.

The following assumes you have:
- [Docker](https://docs.docker.com/engine/install/) installed
- The .NET SDK 8 installed
- A Discord application (via the [Discord developer portal](https://discord.com/developers/applications))

#### Steps

1. Open a shell in the root of the MODiX repository
2. Execute `docker-compose -f dev.docker-compose.yml up -d`
3. Open a shell in `src/MODiX`
4. Execute `dotnet user-secrets set DiscordClientId {CLIENT_ID}` replacing `{CLIENT_ID}` with your Discord application client ID
5. Execute `dotnet user-secrets set DiscordClientSecret {CLIENT_SECRET}` replacing `{CLIENT_SECRET}` with your Discord application client secret
6. Execute `dotnet user-secrets set DiscordToken {TOKEN}` replacing `{TOKEN}` with your Discord bot token
7. Start your IDE and debug or `dotnet run`

### Suggestions, bugs and discussions

All notable work will be documented as GitHub issues. Ensure you do not work on any new features unless it is documented as a GitHub issue, so as to avoid disappointment if any of the core maintainers disagree.

## Notable dependencies

The work we do would not be possible without these notable dependencies. Note that this list is not exhaustive.

- [Discord.NET](https://github.com/discord-net/Discord.Net) - The framework we rely on to communicate with Discord itself
- [Entity Framework Core](https://github.com/dotnet/efcore) - The ORM of choice for all queries
- [efcore.pg](https://github.com/npgsql/efcore.pg) - PostreSQL provider for Entity Framework Core
- [LINQKit](https://github.com/scottksmith95/LINQKit) - Extensions over Entity Framework Core for ergonomic querying
- [Humanizer](https://github.com/Humanizr/Humanizer) - Readable date/time formats for humans, because we're not machines

## Hosting

At this time we do not offer a managed hosted service for MODiX. You can host MODiX along with all of its dependencies using the `docker-stack.yml` file as a template.