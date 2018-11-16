using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix;
using Modix.Adapters.Discord;
using Modix.Behaviors;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services;
using Modix.Services.AutoCodePaste;
using Modix.Services.BehaviourConfiguration;
using Modix.Services.CodePaste;
using Modix.Services.CommandHelp;
using Modix.Services.Core;
using Modix.Services.DocsMaster;
using Modix.Services.GuildInfo;
using Modix.Services.Moderation;
using Modix.Services.PopularityContest;
using Modix.Services.Promotions;
using Modix.Services.Quote;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModix(this IServiceCollection services)
        {
            services.AddSingleton(
                provider => new DiscordSocketClient(config: new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = provider.GetRequiredService<ModixConfig>().MessageCacheSize //needed to log deletions
                }));

            services.AddSingleton<IDiscordClient>(provider => provider.GetRequiredService<DiscordSocketClient>());
            services.AddSingleton<ISelfUser>(p => p.GetRequiredService<DiscordSocketClient>().CurrentUser);

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
                    service.AddModulesAsync(typeof(ModixBot).Assembly).GetAwaiter().GetResult();

                    return service;
                });

            services.AddSingleton<DiscordSerilogAdapter>();
            services.AddSingleton<HttpClient>();
            services.AddMediator();

            services.AddModixCore()
                .AddModixModeration()
                .AddModixPromotions();

            services.AddSingleton<IBehavior, DiscordAdapter>();
            services.AddScoped<IQuoteService, QuoteService>();
            services.AddSingleton<CodePasteHandler>();
            services.AddSingleton<IBehavior, AttachmentBlacklistBehavior>();
            services.AddSingleton<CodePasteService>();
            services.AddScoped<DocsMasterRetrievalService>();
            services.AddMemoryCache();

            services.AddSingleton<GuildInfoService>();
            services.AddSingleton<ICodePasteRepository, MemoryCodePasteRepository>();
            services.AddSingleton<CommandHelpService>();
            services.AddScoped<IPopularityContestService, PopularityContestService>();

            services.AddSingleton<CommandErrorHandler>();
            services.AddScoped<IBehaviourConfigurationRepository, BehaviourConfigurationRepository>();
            services.AddScoped<IBehaviourConfigurationService, BehaviourConfigurationService>();
            services.AddSingleton<IBehaviourConfiguration, BehaviourConfiguration>();

            services.AddScoped<IModerationActionEventHandler, ModerationLoggingBehavior>()
                .AddScoped<IPromotionActionEventHandler, PromotionLoggingBehavior>();

            services.AddHostedService<ModixBot>();

            return services;
        }
    }
}
