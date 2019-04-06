using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Modix
{
    public class DiscordSerilogAdapter
    {
        private readonly ILogger _log;

        public DiscordSerilogAdapter(ILogger<DiscordSerilogAdapter> log)
        {
            _log = log;
        }

        public Task HandleLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _log.LogCritical(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Debug:
                    _log.LogDebug(message.ToString());
                    break;
                case LogSeverity.Warning:
                    _log.LogWarning(message.ToString());
                    break;
                case LogSeverity.Error:
                    _log.LogError(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Info:
                    _log.LogInformation(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    _log.LogTrace(message.ToString());
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
