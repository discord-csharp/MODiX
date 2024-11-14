using System;
using System.Net;
using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Modix.Behaviors;
using Modix.Bot;
using Modix.Bot.Behaviors;
using Modix.Bot.Responders.AutoRemoveMessages;
using Modix.Bot.Responders.CommandErrors;
using Modix.Bot.Responders.MessageQuotes;
using Modix.Common;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.EmojiStats;
using Modix.Services.GuildStats;
using Modix.Services.Images;
using Modix.Services.Moderation;
using Modix.Services.Promotions;
using Modix.Services.Starboard;
using Modix.Services.Tags;
using Modix.Services.Utilities;
using Polly;
using Polly.Extensions.Http;

namespace Modix.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModixHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddHttpClient(HttpClientNames.RetryOnTransientErrorPolicy)
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(5)));

        services.AddHttpClient(HttpClientNames.TimeoutFiveSeconds)
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            });

        services.AddHttpClient(HttpClientNames.Timeout300ms)
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(300);
            });

        services.AddHttpClient(HttpClientNames.AutomaticGZipDecompression)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                });

        return services;
    }

    public static IServiceCollection AddModix(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSingleton(
                provider => new DiscordSocketClient(config: new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    GatewayIntents =
                        GatewayIntents.GuildBans |              // GUILD_BAN_ADD, GUILD_BAN_REMOVE
                        GatewayIntents.GuildMembers |           // GUILD_MEMBER_ADD, GUILD_MEMBER_UPDATE, GUILD_MEMBER_REMOVE
                        GatewayIntents.GuildMessageReactions |  // MESSAGE_REACTION_ADD, MESSAGE_REACTION_REMOVE,
                        //     MESSAGE_REACTION_REMOVE_ALL, MESSAGE_REACTION_REMOVE_EMOJI
                        GatewayIntents.GuildMessages |          // MESSAGE_CREATE, MESSAGE_UPDATE, MESSAGE_DELETE, MESSAGE_DELETE_BULK
                        GatewayIntents.Guilds |                 // GUILD_CREATE, GUILD_UPDATE, GUILD_DELETE, GUILD_ROLE_CREATE,
                        //     GUILD_ROLE_UPDATE, GUILD_ROLE_DELETE, CHANNEL_CREATE,
                        //     CHANNEL_UPDATE, CHANNEL_DELETE, CHANNEL_PINS_UPDATE
                        GatewayIntents.MessageContent,          // MESSAGE_CONTENT
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = provider
                        .GetRequiredService<IOptions<ModixConfig>>()
                        .Value
                        .MessageCacheSize //needed to log deletions
                }))
            .AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());

        services
            .AddSingleton(
                provider => new DiscordRestClient(config: new DiscordRestConfig
                {
                    LogLevel = LogSeverity.Debug,
                }));

        services.AddSingleton(_ =>
            {
                var service = new CommandService(
                    new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        DefaultRunMode = Discord.Commands.RunMode.Sync,
                        CaseSensitiveCommands = false,
                        SeparatorChar = ' '
                    });

                service.AddTypeReader<IEmote>(new EmoteTypeReader());
                service.AddTypeReader<DiscordUserEntity>(new UserEntityTypeReader());
                service.AddTypeReader<AnyGuildMessage<IUserMessage>>(new AnyGuildMessageTypeReader<IUserMessage>());
                service.AddTypeReader<TimeSpan>(new TimeSpanTypeReader(), true);
                service.AddTypeReader<DiscordUserOrMessageAuthorEntity>(new UserOrMessageAuthorEntityTypeReader());
                service.AddTypeReader<Uri>(new UriTypeReader());

                return service;
            });

        services.AddSingleton(provider =>
            {
                var socketClient = provider.GetRequiredService<DiscordSocketClient>();
                var service = new InteractionService(socketClient, new()
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = Discord.Interactions.RunMode.Sync,
                    UseCompiledLambda = true,
                    AutoServiceScopes = false,
                });

                service.AddTypeConverter<IEmote>(new EmoteTypeConverter());
                service.AddTypeConverter<Uri>(new Modix.Bot.TypeConverters.UriTypeConverter());

                return service;
            })
            .AddScoped<Modix.Common.Messaging.INotificationHandler<InteractionCreatedNotification>, InteractionListeningBehavior>();

        services.AddSingleton<DiscordSerilogAdapter>();

        services
            .AddModixCommon(configuration)
            .AddModixServices(configuration)
            .AddModixBot(configuration)
            .AddModixCore()
            .AddModixModeration()
            .AddModixPromotions()
            .AddCommandHelp()
            .AddGuildStats()
            .AddModixTags()
            .AddStarboard()
            .AddEmojiStats()
            .AddImages();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModixBot).Assembly));
        services.AddScoped<AutoRemoveMessageService>();
        services.AddScoped<MessageQuoteEmbedHelper>();
		services.AddScoped<PasteService>();
		services.AddScoped<CommandErrorService>();
		services.AddScoped<DiscordRelayService>();
		services.AddScoped<GuildOnboardingService>();
		services.AddScoped<AuthorizationClaimService>();
		services.AddScoped<AuthorizationClaimMappingService>();
		services.AddScoped<IScopedSession, DiscordBotSession>();

        services.AddMemoryCache();

        services.AddScoped<IModerationActionEventHandler, ModerationLoggingBehavior>();
        services.AddScoped<INotificationHandler<PromotionActionCreatedNotification>, PromotionLoggingHandler>();

        services.AddHostedService<ModixBot>();

        return services;
    }
}
