Modix has grown a bit since the initial implementation. It now consists of multiple components, each with their own dependencies - as well as various test projects.

# `~/Modix` - Core application, HTTP API
This is Modix proper - the startup project. Written in C# and targeting .NET Core, it has cross-project dependencies on `~/Modix.Bot`, `~/Modix.Data`, and `~/Modix.Services` (as defined by `Modix.sln`), and contains the core logic for the HTTP API, which is driven by ASP.NET Core. This includes all the controllers, HTTP-specific authentication logic, web frontend models, and the ASP.NET Core startup class.

## `~/Modix/ClientApp` - Web Frontend
This is the web frontend for Modix, written in Typescript using Vue.JS and associated tooling (`vue-cli`, `webpack`, `npm`, etc).

## `~/Modix.Bot` - Discord Bot
This project contains the Discord.Net-specific and core Modix bot stuff, such as command modules and type readers. It's contained within a `BackgroundService` to be hosted within the core application.

## `~/Modix.Services` - Service Layer
This project contains the majority of our business logic types, and is used by `~/Modix.Bot` (the Discord frontend) and `~/Modix` (core / HTTP). If a command is run from either of these frontends, it will probably be through a service within this project.

## `~/Modix.Data` - Data Layer
This is our data layer, and contains the majority of the POCO models and database logic, as well as migrations. This includes the database context, driven by Entity Framework Core and using PostgreSQL.

## `~/Modix.Common` - Common Stuff
This is where we put stuff that is so generic that any other project can / will depend on it.

# CSDiscord
This is a separate project, [located here](https://github.com/discord-csharp/CSDiscord), which powers our REPL/eval feature. Combined with kubernetes orchestration, we follow a "security by disposability" pattern, where containers are spun up and disposed of as eval requests come in. Communication to the service from Modix itself is over HTTP - see `ReplModule.cs` in the main Modix project.