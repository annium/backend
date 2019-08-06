using System;
using System.Text;

namespace Annium.Testing.Logging
{
    internal class Logger : ILogger
    {
        private static readonly object consoleLock = new object();

        private readonly LoggerConfiguration configuration;

        public Logger(
            TestingConfiguration configuration
        )
        {
            this.configuration = configuration.Logger;
        }

        public void LogTrace(string message) => Log(LogLevel.Trace, message);

        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        public void LogInfo(string message) => Log(LogLevel.Info, message);

        public void LogWarn(string message) => Log(LogLevel.Warn, message);

        public void LogError(Exception exception) => Log(LogLevel.Error, exception);

        private void Log(LogLevel level, string message)
        {
            if (level < configuration.LogLevel)
                return;

            lock(consoleLock)
            WriteLine(level, message);
        }

        private void Log(LogLevel level, Exception exception)
        {
            if (level < configuration.LogLevel)
                return;

            lock(consoleLock)
            {
                if (exception is AggregateException aggregateException)
                {
                    var errors = aggregateException.Flatten().InnerExceptions;
                    WriteLine(level, $"Errors ({errors.Count}):");

                    foreach (var error in errors)
                        WriteLine(level, getExceptionMessage(error));
                }
                else
                    WriteLine(level, getExceptionMessage(exception));
            }

            string getExceptionMessage(Exception e) => configuration.LogLevel > LogLevel.Debug ?
                exception.Message :
                $"{exception.Message}{Environment.NewLine}{exception.StackTrace}";
        }

        private void WriteLine(LogLevel logLevel, string message)
        {
            var builder = new StringBuilder();

            builder.Append(message);

            Console.WriteLine(builder.ToString());
        }
    }
}