using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace UCSimpleSocket.IntegrationTests
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger Create(string name)
        {
            return new VSDebugLogger();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new VSDebugLogger();
        }

        public void Dispose()
        {
        }

        public class VSDebugLogger : ILogger<Connection>
        {
            public string Name { get; set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Debug.WriteLine(Name + ":\t" + formatter(state, exception));
            }
        }
    }
}