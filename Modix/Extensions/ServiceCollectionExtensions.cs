using System;
using System.Net;
using System.Net.Http;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Modix;
using Modix.Behaviors;
using Modix.Bot;
using Modix.Bot.Behaviors;
using Modix.Common;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.DataDog;
using Modix.RemoraShim;
using Modix.Services;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Csharp;
using Modix.Services.DocsMaster;
using Modix.Services.EmojiStats;
using Modix.Services.Giveaways;
using Modix.Services.GuildStats;
using Modix.Services.Images;
using Modix.Services.Moderation;
using Modix.Services.PopularityContest;
using Modix.Services.Promotions;
using Modix.Services.Quote;
using Modix.Services.StackExchange;
using Modix.Services.Starboard;
using Modix.Services.Tags;
using Modix.Services.Utilities;
using Modix.Services.Wikipedia;

using Polly;
using Polly.Extensions.Http;

using StatsdClient;

namespace Microsoft.Extensions.DependencyInjection
{
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
                            GatewayIntents.Guilds,                  // GUILD_CREATE, GUILD_UPDATE, GUILD_DELETE, GUILD_ROLE_CREATE,
                                                                    //     GUILD_ROLE_UPDATE, GUILD_ROLE_DELETE, CHANNEL_CREATE,
                                                                    //     CHANNEL_UPDATE, CHANNEL_DELETE, CHANNEL_PINS_UPDATE
                        LogLevel = LogSeverity.Debug,
                        MessageCacheSize = provider
                            .GetRequiredService<IOptions<ModixConfig>>()
                            .Value
                            .MessageCacheSize //needed to log deletions
                    }))
                .AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>())
                .AddSingleton(provider => provider.GetRequiredService<DiscordSocketClient>().Abstract());

            services
                .AddSingleton(
                    provider => new DiscordRestClient(config: new DiscordRestConfig
                    {
                        LogLevel = LogSeverity.Debug,
                    }))
                .AddSingleton(provider => provider.GetRequiredService<DiscordRestClient>().Abstract());

            services.AddSingleton(_ =>
                {
                    var service = new CommandService(
                        new CommandServiceConfig
                        {
                            LogLevel = LogSeverity.Debug,
                            DefaultRunMode = RunMode.Sync,
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
                })
                .AddScoped<Modix.Common.Messaging.INotificationHandler<MessageReceivedNotification>, CommandListeningBehavior>();

            services.AddSingleton<DiscordSerilogAdapter>();

            services
                .AddModixCommon(configuration)
                .AddModixServices(configuration)
                .AddModixBot(configuration)
                .AddModixCore()
                .AddModixModeration()
                .AddModixPromotions()
                .AddCodePaste()
                .AddCommandHelp()
                .AddGuildStats()
                .AddModixTags()
                .AddStarboard()
                .AddAutoRemoveMessage()
                .AddEmojiStats()
                .AddImages()
                .AddGiveaways();

            services.AddScoped<IQuoteService, QuoteService>();
            services.AddSingleton<IBehavior, MessageLinkBehavior>();
            services.AddScoped<DocsMasterRetrievalService>();
            services.AddMemoryCache();

            services.AddScoped<IPopularityContestService, PopularityContestService>();
            services.AddScoped<WikipediaService>();
            services.AddScoped<StackExchangeService>();
            services.AddScoped<DocumentationService>();

            services.AddScoped<IModerationActionEventHandler, ModerationLoggingBehavior>();
            services.AddScoped<INotificationHandler<PromotionActionCreatedNotification>, PromotionLoggingHandler>();

            services.AddHostedService<ModixBot>();

            services.AddRemoraShim(configuration);

            return services;
        }

        public static IServiceCollection AddStatsD(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
        {
            var cfg = new StatsdConfig { Prefix = "modix" };

            var enableStatsd = configuration.GetValue<bool>(nameof(ModixConfig.EnableStatsd));
            if (!enableStatsd ||
                !environment.IsProduction() && string.IsNullOrWhiteSpace(cfg.StatsdServerName))
            {
                services.AddSingleton<IDogStatsd, DebugDogStatsd>();
                return services;
            }

            DogStatsd.Configure(cfg);
            services.AddSingleton(cfg);
            services.AddSingleton<IDogStatsd>(provider =>
            {
                var config = provider.GetRequiredService<StatsdConfig>();
                var service = new DogStatsdService();
                service.Configure(config);
                return service;
            });
            return services;
        }
    }
}
