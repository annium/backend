using System;
using System.Collections.Generic;
using System.Text;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Console
{
    internal class ConsoleLogger<T> : BaseLogger<T> where T : class
    {
        private static readonly object consoleLock = new object();

        private readonly IReadOnlyDictionary<LogLevel, ConsoleColor> levelColors;

        public ConsoleLogger(
            LoggerConfiguration configuration,
            Func<Instant> getInstant
        ) : base(configuration, getInstant)
        {
            var levelColors = new Dictionary<LogLevel, ConsoleColor>();
            levelColors[LogLevel.Trace] = ConsoleColor.DarkGray;
            levelColors[LogLevel.Debug] = ConsoleColor.Gray;
            levelColors[LogLevel.Info] = ConsoleColor.White;
            levelColors[LogLevel.Warn] = ConsoleColor.Yellow;
            levelColors[LogLevel.Error] = ConsoleColor.Red;
            this.levelColors = levelColors;
        }

        protected override void LogMessage(Instant instant, LogLevel level, string message)
        {
            lock(consoleLock)
            {
                var currentColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = levelColors[level];
                WriteLine(instant, level, message);
                System.Console.ForegroundColor = currentColor;
            }
        }

        protected override void LogException(Instant instant, LogLevel level, Exception exception)
        {
            lock(consoleLock)
            {
                var currentColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = levelColors[level];

                if (exception is AggregateException aggregateException)
                {
                    var errors = aggregateException.Flatten().InnerExceptions;
                    WriteLine(instant, level, $"Errors ({errors.Count}):");

                    foreach (var error in errors)
                        WriteLine(instant, level, getExceptionMessage(error));
                }
                else
                    WriteLine(instant, level, getExceptionMessage(exception));

                System.Console.ForegroundColor = currentColor;
            }

            string getExceptionMessage(Exception e) => configuration.LogLevel > LogLevel.Debug ?
                exception.Message :
                $"{exception.Message}{Environment.NewLine}{exception.StackTrace}";
        }

        private void WriteLine(Instant instant, LogLevel logLevel, string message)
        {
            var builder = new StringBuilder();

            builder.Append($"[{instant.InZone(tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] ");

            builder.Append(message);

            System.Console.WriteLine(builder.ToString());
        }
    }
}