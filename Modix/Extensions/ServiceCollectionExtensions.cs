﻿using System;
using System.Net;
using System.Net.Http;

using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using Microsoft.Extensions.Options;

using Modix;
using Modix.Behaviors;
using Modix.Bot;
using Modix.Bot.Behaviors;
using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services;
using Modix.Services.AutoRemoveMessage;
using Modix.Services.BehaviourConfiguration;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.Csharp;
using Modix.Services.DocsMaster;
using Modix.Services.EmojiStats;
using Modix.Services.Giveaways;
using Modix.Services.GuildStats;
using Modix.Services.Images;
using Modix.Services.Mentions;
using Modix.Services.Moderation;
using Modix.Services.PopularityContest;
using Modix.Services.Promotions;
using Modix.Services.Quote;
using Modix.Services.StackExchange;
using Modix.Services.Starboard;
using Modix.Services.Tags;
using Modix.Services.Wikipedia;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModixHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddHttpClient(nameof(CodePasteService))
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                });

            services.AddHttpClient(nameof(StackExchangeService))
                .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                });

            return services;
        }

        public static IServiceCollection AddModix(this IServiceCollection services)
        {
            services.AddSingleton(
                provider => new DiscordSocketClient(config: new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = provider
                        .GetRequiredService<IOptions<ModixConfig>>()
                        .Value
                        .MessageCacheSize //needed to log deletions
                }));

            services.AddSingleton<IDiscordSocketClient>(provider => provider.GetRequiredService<DiscordSocketClient>().Abstract());
            services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());

            services.AddSingleton(
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
                            DefaultRunMode = RunMode.Sync,
                            CaseSensitiveCommands = false,
                            SeparatorChar = ' '
                        });

                    service.AddTypeReader<IEmote>(new EmoteTypeReader());
                    service.AddTypeReader<DiscordUserEntity>(new UserEntityTypeReader());
                    service.AddTypeReader<AnyGuildMessage<IUserMessage>>(new AnyGuildMessageTypeReader<IUserMessage>());
                    service.AddTypeReader<TimeSpan>(new TimeSpanTypeReader(), true);

                    return service;
                })
                .AddScoped<Modix.Common.Messaging.INotificationHandler<MessageReceivedNotification>, CommandListeningBehavior>();

            services.AddSingleton<DiscordSerilogAdapter>();

            services
                .AddModixCore()
                .AddModixMessaging()
                .AddModixModeration()
                .AddModixPromotions()
                .AddCodePaste()
                .AddCommandHelp()
                .AddGuildStats()
                .AddMentions()
                .AddModixTags()
                .AddStarboard()
                .AddAutoRemoveMessage()
                .AddEmojiStats()
                .AddImages()
                .AddGiveaways();

            services.AddScoped<IQuoteService, QuoteService>();
            services.AddSingleton<IBehavior, MessageLinkBehavior>();
            services.AddSingleton<IBehavior, AttachmentBlacklistBehavior>();
            services.AddScoped<DocsMasterRetrievalService>();
            services.AddMemoryCache();

            services.AddScoped<IPopularityContestService, PopularityContestService>();
            services.AddScoped<WikipediaService>();
            services.AddScoped<StackExchangeService>();
            services.AddScoped<DocumentationService>();

            services.AddScoped<IBehaviourConfigurationRepository, BehaviourConfigurationRepository>();
            services.AddScoped<IBehaviourConfigurationService, BehaviourConfigurationService>();
            services.AddSingleton<IBehaviourConfiguration, BehaviourConfiguration>();

            services.AddScoped<IModerationActionEventHandler, ModerationLoggingBehavior>();
            services.AddScoped<INotificationHandler<PromotionActionCreatedNotification>, PromotionLoggingHandler>();

            services.AddHostedService<ModixBot>();

            return services;
        }
    }
}
