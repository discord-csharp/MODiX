using System;
using System.Diagnostics;
using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Models.Core;
using Modix.Services.CodePaste;
using Modix.Services.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Modix
{
    public class Program
    {
        public static int Main(string[] args)
        {
            const string DEVELOPMENT_ENVIRONMENT_VARIABLE = "ASPNETCORE_ENVIRONMENT";
            const string DEVELOPMENT_ENVIRONMENT_KEY = "Development";

            var environment = Environment.GetEnvironmentVariable(DEVELOPMENT_ENVIRONMENT_VARIABLE);

            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables("MODIX_")
                .AddJsonFile("developmentSettings.json", optional: true, reloadOnChange: false)
                .AddKeyPerFile("/run/secrets", true);

            if(environment is DEVELOPMENT_ENVIRONMENT_KEY)
            {
                configBuilder.AddUserSecrets<Program>();
            }

            var config = configBuilder.Build();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Modix.DiscordSerilogAdapter", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Logger(subLoggerConfig => subLoggerConfig
                    .MinimumLevel.Information()
                    // .MinimumLevel.Override() is not supported for sub-loggers, even though the docs don't specify this. See https://github.com/serilog/serilog/pull/1033
                    .Filter.ByExcluding("SourceContext like 'Microsoft.%' and @Level in ['Information', 'Debug', 'Verbose']")
                    .WriteTo.Console()
                    .WriteTo.RollingFile(Path.Combine("logs", "{Date}.log")))
                .WriteTo.RollingFile(
                    new RenderedCompactJsonFormatter(),
                    Path.Combine("logs", "{Date}.clef"),
                    retainedFileCountLimit: 2);

            var seqEndpoint = config.GetValue<string>(nameof(ModixConfig.SeqEndpoint));
            if (!string.IsNullOrWhiteSpace(seqEndpoint))
            { 
                loggerConfig = loggerConfig.WriteTo.Seq(seqEndpoint, LogEventLevel.Information);
            }

            var webhookId = config.GetValue<ulong>(nameof(ModixConfig.LogWebhookId));
            var webhookToken = config.GetValue<string>(nameof(ModixConfig.LogWebhookToken));

            var webHost = CreateWebHostBuilder(args, config).Build();

            if (webhookId != default && string.IsNullOrWhiteSpace(webhookToken) == false)
            {
                loggerConfig = loggerConfig
                    .WriteTo.DiscordWebhookSink(webhookId, webhookToken, LogEventLevel.Error, webHost.Services.GetRequiredService<CodePasteService>());
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.ForContext<Program>()
                    .Fatal(ex, "Host terminated unexpectedly.");

                if (Debugger.IsAttached && Environment.UserInteractive)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to exit...");
                    Console.ReadKey(true);
                }

                return ex.HResult;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseSerilog()
                .UseStartup<Startup>();
    }
}
