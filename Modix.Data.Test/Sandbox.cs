using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NSubstitute;
using Shouldly;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Modix.Data;
using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

namespace Modix.Data.Test
{
    [TestFixture]
    public class Sandbox
    {
        [Test]
        public async Task Main()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModixContext>();

            var loggerFactory = Substitute.For<ILoggerFactory>();
            loggerFactory.CreateLogger(Arg.Any<string>()).Returns((x) => new ConsoleLogger());

            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=postgres;User Id=X;Password=X;");
            optionsBuilder.UseLoggerFactory(loggerFactory);

            using (var context = new ModixContext(optionsBuilder.Options))
            {
            }
        }
    }

    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
            => Substitute.For<IDisposable>();

        public bool IsEnabled(LogLevel logLevel)
            => true;

        private HashSet<string> _eventIdNames
            = new HashSet<string>()
            {
                "Microsoft.EntityFrameworkCore.Query.QueryClientEvaluationWarning",
                "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted"
            };

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_eventIdNames.Any() && !_eventIdNames.Contains(eventId.Name))
                return;

            Console.WriteLine($" ~~~~ {logLevel.ToString()} ~~~~ {eventId.Name}\r\n{formatter(state, exception)}\r\n");
        }
    }
}
